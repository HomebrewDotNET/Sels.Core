using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Distributed;
using Sels.Core.Conversion.Converters;
using Sels.Core.ServiceBuilder.Template.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Extensions.Linq;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Logging.Advanced;

namespace Sels.Core.ServiceBuilder.Interceptors
{
    /// <summary>
    /// Interceptor that caches the return values of methods.
    /// </summary>
    public class CachingInterceptor : BaseResultOnlyInterceptor, ICachingInterceptorBuilder
    {
        // Fields
        private readonly IDistributedCache _cache;
        private readonly List<MethodCacher> _cachers = new List<MethodCacher>();
        private readonly ILoggerFactory? _factory;
        private readonly IEnumerable<ILogger?>? _loggers;
        private readonly ITypeConverter _converter;

        // State
        private Func<object, string> _serializeFunc;
        private Func<string, Type, object> _deserializeFunc;
        private Func<IInvocation, string> _keyGetter;
        private Func<IInvocation, DistributedCacheEntryOptions> _optionGetter;

        /// <inheritdoc cref="CachingInterceptor"/>
        /// <param name="cache">The cache to use to store method return values</param>
        /// <param name="defaultConverter">Optional default converter for serializing and deserializing method return values</param>
        /// <param name="loggers">Static loggers to use for tracing</param>
        public CachingInterceptor(IDistributedCache cache, ITypeConverter? defaultConverter, IEnumerable<ILogger?>? loggers) : this(cache, defaultConverter)
        {
            _loggers = loggers != null ? loggers.Where(x => x != null) : null;
        }

        /// <inheritdoc cref="CachingInterceptor"/>
        /// <param name="cache">The cache to use to store method return values</param>
        /// <param name="defaultConverter">Optional default converter for serializing and deserializing method return values</param>
        /// <param name="factory">Logger factory for creating loggers based on the target type</param>
        public CachingInterceptor(IDistributedCache cache, ITypeConverter? defaultConverter, ILoggerFactory? factory) : this(cache, defaultConverter)
        {
            _factory = factory;
        }

        private CachingInterceptor(IDistributedCache cache, ITypeConverter? defaultConverter)
        {
            _cache = cache.ValidateArgument(nameof(cache));
            _converter = defaultConverter ?? GenericConverter.DefaultJsonConverter;

            // Set default settings
            var builder = this.Cast<ICachingInterceptorBuilder>();
            builder.ConvertUsingDefault(_converter)
                   .GetKeyWithDefault(GetDefaultKey)
                   .WithDefaultOptions(GetDefaultOptions);
        }

        /// <inheritdoc/>
        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            var loggers = GetLoggersFor(invocation);

            var cacher = _cachers.FirstOrDefault(x => x.CanCache(invocation));
            var methodName = invocation.MethodInvocationTarget.GetDisplayName(true);

            if(cacher != null)
            {
                loggers.Debug($"Caching enabled for <{methodName}>");
                var key = (cacher.KeyGetter ?? _keyGetter).Invoke(invocation);
                if (!key.HasValue()) throw new InvalidOperationException($"Caching key cannot be null, empty or whitespace for method <{methodName}>");
                var token = invocation.Arguments.FirstOrDefault(x => x is CancellationToken).CastOrDefault<CancellationToken>();

                loggers.Debug($"Checking cache for <{key}>");
                var bytes = await _cache.GetAsync(key, token);

                if (bytes.HasValue())
                {
                    loggers.Debug($"Got cached value of size <{bytes.Length}> for <{key}>. Deserializing");
                    var cachedString = bytes.ToString<UTF8Encoding>();
                    loggers.TraceObject($"Retrieved cached string for <{key}>:", cachedString);
                    var result = (cacher.DeserializeFunc ?? _deserializeFunc).Invoke(cachedString, typeof(TResult));
                    loggers.TraceObject($"Retrieved cached object for <{key}>:", result);
                    return result.CastOrDefault<TResult>();
                }
                else
                {
                    loggers.Debug($"No value cached for <{key}>. Calling method");
                    var result = await proceed(invocation, proceedInfo);
                    if(result != null)
                    {
                        loggers.TraceObject($"Retrieved object from method call for <{key}>:", result);
                        var serialized = (cacher.SerializeFunc ?? _serializeFunc).Invoke(result);
                        loggers.TraceObject($"Created serialized string for caching <{key}>", serialized);
                        bytes = serialized.GetBytes<UTF8Encoding>();
                        var options = (cacher.OptionGetter ?? _optionGetter).Invoke(invocation);
                        loggers.Debug($"Setting cache value of size <{bytes.Length}> for <{key}>");
                        await _cache.SetAsync(key, bytes, options, token);
                    }
                    else
                    {
                        loggers.Warning($"Method <{methodName}> returned null so no value to cache. Returning method return value");
                    }
                    return result;
                }
            }
            else
            {
                loggers.Debug($"Caching not enabled for <{methodName}>. Skipping");

                return await proceed(invocation, proceedInfo);
            }
        }

        #region Building
        /// <inheritdoc/>
        public ICachingMethodInterceptorBuilder Method(MethodInfo method)
        {
            method.ValidateArgument(nameof(method));

            var cacher = new MethodCacher(this, method);
            _cachers.Add(cacher);
            return cacher;
        }
        /// <inheritdoc/>
        public ICachingInterceptorBuilder ConvertUsingDefault(Func<object, string> serializeFunc, Func<string, Type, object> deserializeFunc)
        {
            _serializeFunc = serializeFunc.ValidateArgument(nameof(serializeFunc));
            _deserializeFunc = deserializeFunc.ValidateArgument(nameof(deserializeFunc));

            return this;
        }
        /// <inheritdoc/>
        public ICachingInterceptorBuilder GetKeyWithDefault(Func<IInvocation, string> keyGetter)
        {
            _keyGetter = keyGetter.ValidateArgument(nameof(keyGetter));

            return this;
        }
        /// <inheritdoc/>
        public ICachingInterceptorBuilder WithDefaultOptions(Func<IInvocation, DistributedCacheEntryOptions> optionGetter)
        {
            _optionGetter = optionGetter.ValidateArgument(nameof(optionGetter));
            return this;
        }

        private class MethodCacher : ICachingMethodInterceptorBuilder
        {
            // Fields
            private Predicate<IInvocation> _condition;

            // Properties
            public ICachingInterceptorBuilder And { get; }
            public MethodInfo Method { get; }

            public Func<object, string> SerializeFunc { get; private set; }
            public Func<string, Type, object> DeserializeFunc { get; private set; }
            public Func<IInvocation, string> KeyGetter { get; private set; }
            public Func<IInvocation, DistributedCacheEntryOptions> OptionGetter { get; private set; }

            public MethodCacher(ICachingInterceptorBuilder parent, MethodInfo method)
            {
                And = parent.ValidateArgument(nameof(parent));
                Method = method.ValidateArgument(nameof(method));
            }

            public bool CanCache(IInvocation invocation)
            {
                invocation.ValidateArgument(nameof(invocation));

                return invocation.MethodInvocationTarget.AreEqual(Method) && (_condition == null || _condition(invocation));
            }

            /// <inheritdoc/>
            public ICachingMethodInterceptorBuilder ConvertUsing(Func<object, string> serializeFunc, Func<string, Type, object> deserializeFunc)
            {
                SerializeFunc = serializeFunc.ValidateArgument(nameof(serializeFunc));
                DeserializeFunc = deserializeFunc.ValidateArgument(nameof(deserializeFunc));

                return this;
            }
            /// <inheritdoc/>
            public ICachingMethodInterceptorBuilder When(Predicate<IInvocation> condition)
            {
                _condition = condition.ValidateArgument(nameof(condition));
                return this;
            }
            /// <inheritdoc/>
            public ICachingMethodInterceptorBuilder WithOptions(Func<IInvocation, DistributedCacheEntryOptions> optionGetter)
            {
                OptionGetter = optionGetter.ValidateArgument(nameof(optionGetter));
                return this;
            }
            /// <inheritdoc/>
            public ICachingMethodInterceptorBuilder WithKey(Func<IInvocation, string> keyGetter)
            {
                KeyGetter = keyGetter.ValidateArgument(nameof(keyGetter));

                return this;
            }


        }
        #endregion

        private string GetDefaultKey(IInvocation invocation)
        {
            return invocation.MethodInvocationTarget.GetDisplayName(true, invocation != null ? invocation.Arguments.Select(x => _converter.ConvertTo<string>(x)).ToArray() : null);
        }

        private DistributedCacheEntryOptions GetDefaultOptions(IInvocation invocation)
        {
            return new DistributedCacheEntryOptions() { SlidingExpiration = TimeSpan.FromMinutes(5) };
        }

        private IEnumerable<ILogger> GetLoggersFor(IInvocation invocation)
        {
            if (_factory != null)
            {
                return _factory.CreateLogger(invocation.Proxy.GetType()).AsEnumerable();
            }

            return _loggers;
        }
    }
}
