using Castle.DynamicProxy;
using Sels.Core.Conversion.Converters;
using Sels.Core.ServiceBuilder.Template.Interceptors;
using System.Reflection;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Extensions.Linq;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging.Advanced;

namespace Sels.Core.ServiceBuilder.Interceptors.Caching
{
    /// <summary>
    /// Interceptor that caches the return values of methods.
    /// </summary>
    /// <typeparam name="TImpl">The type of the object to cache values on</typeparam>
    /// <typeparam name="TOptions">The builder that exposes extra cahcing options</typeparam>
    public class CachingInterceptor<TImpl, TOptions> : BaseResultOnlyInterceptor, ICachingInterceptorBuilder<TImpl, TOptions>
    {
        // Fields
        private readonly List<MethodCacher> _cachers = new List<MethodCacher>();
        private readonly IInterceptorCachingProvider<TOptions> _cacheProvider;
        private readonly ITypeConverter? _typeConverter;
        private readonly ILoggerFactory? _factory;
        private readonly ILogger<CachingInterceptor<TImpl, TOptions>>? _logger;

        // State
        private Func<IInvocation, string> _keyGetter;
        private Action<IInvocation, TOptions>? _optionBuilder;

        /// <inheritdoc cref="CachingInterceptor{TImpl, TOptions}"/>
        /// <param name="cacheProvider"><inheritdoc cref="IInterceptorCachingProvider{TOptions}"/></param>
        /// <param name="typeConverter">Optional type converter that is used to convert the method parameters to strings. If not provided <see cref="object.ToString"/> will be used</param>
        /// <param name="loggerFactory">Optional factory that creates logger specifically for the target class</param>
        /// <param name="logger">Optional logger for tracing</param>
        public CachingInterceptor(IInterceptorCachingProvider<TOptions> cacheProvider, ITypeConverter? typeConverter = null, ILoggerFactory? loggerFactory = null, ILogger<CachingInterceptor<TImpl, TOptions>>? logger = null)
        {
            this._cacheProvider = Guard.IsNotNull(cacheProvider);
            this._typeConverter = typeConverter;
            _factory = loggerFactory;
            this._logger = logger;

            // Set default key getter
            GetKeyWithDefault(GetDefaultKey);
        }


        /// <inheritdoc/>
        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            var logger = GetLoggerFor(invocation);

            var cacher = _cachers.FirstOrDefault(x => x.CanCache(invocation));
            var methodName = invocation.MethodInvocationTarget.GetDisplayName(true);

            if (cacher != null)
            {
                logger.Debug($"Caching enabled for <{methodName}>");
                var key = (cacher.KeyGetter ?? _keyGetter).Invoke(invocation);
                if (!key.HasValue()) throw new InvalidOperationException($"Caching key cannot be null, empty or whitespace for method <{methodName}>");
                var token = invocation.Arguments.FirstOrDefault(x => x is CancellationToken).CastOrDefault<CancellationToken>();

                logger.Debug($"Checking cache for <{key}>");

                TOptions options = default;
                var optionBuilder = (cacher?.OptionBuilder ?? _optionBuilder);
                if(optionBuilder != null)
                {
                    options = _cacheProvider.CreateNewOptions();
                    optionBuilder(invocation, options);
                    logger.Trace($"Created caching options <{options}> for method <{methodName}>");
                }

                var returned = await _cacheProvider.GetOrSetAsync<TResult>(invocation, key, options, x => proceed(invocation, proceedInfo), token);
                invocation.ReturnValue = returned;
                return returned;
            }
            else
            {
                logger.Debug($"Caching not enabled for <{methodName}>. Skipping");

                return await proceed(invocation, proceedInfo);
            }
        }

        #region Building
        /// <inheritdoc/>
        public ICachingMethodInterceptorBuilder<TImpl, TOptions> Method(MethodInfo method)
        {
            method.ValidateArgument(nameof(method));

            var cacher = new MethodCacher(this, method);
            _cachers.Add(cacher);
            return cacher;
        }

        /// <inheritdoc/>
        public ICachingInterceptorBuilder<TImpl, TOptions> GetKeyWithDefault(Func<IInvocation, string> keyGetter)
        {
            _keyGetter = keyGetter.ValidateArgument(nameof(keyGetter));

            return this;
        }
        /// <inheritdoc/>
        public ICachingInterceptorBuilder<TImpl, TOptions> WithDefaultOptions(Action<IInvocation, TOptions> optionGetter)
        {
            _optionBuilder = optionGetter.ValidateArgument(nameof(optionGetter));
            return this;
        }

        private class MethodCacher : ICachingMethodInterceptorBuilder<TImpl, TOptions>
        {
            // Fields
            private Predicate<IInvocation> _condition;

            // Properties
            public ICachingInterceptorBuilder<TImpl, TOptions> And { get; }
            public MethodInfo Method { get; }

            public Func<IInvocation, string>? KeyGetter { get; private set; }
            public Action<IInvocation, TOptions>? OptionBuilder { get; private set; }

            public MethodCacher(ICachingInterceptorBuilder<TImpl, TOptions> parent, MethodInfo method)
            {
                And = parent.ValidateArgument(nameof(parent));
                Method = method.ValidateArgument(nameof(method));
            }

            public bool CanCache(IInvocation invocation)
            {
                invocation.ValidateArgument(nameof(invocation));

                return invocation.Method.AreEqual(Method) && (_condition == null || _condition(invocation));
            }

            /// <inheritdoc/>
            public ICachingMethodInterceptorBuilder<TImpl, TOptions> When(Predicate<IInvocation> condition)
            {
                _condition = condition.ValidateArgument(nameof(condition));
                return this;
            }
            /// <inheritdoc/>
            public ICachingMethodInterceptorBuilder<TImpl, TOptions> WithKey(Func<IInvocation, string> keyGetter)
            {
                KeyGetter = keyGetter.ValidateArgument(nameof(keyGetter));

                return this;
            }

            /// <inheritdoc/>
            public ICachingMethodInterceptorBuilder<TImpl, TOptions> WithOptions(Action<IInvocation, TOptions> optionBuilder)
            {
                OptionBuilder = Guard.IsNotNull(optionBuilder);
                return this;
            }
        }
        #endregion

        private string GetDefaultKey(IInvocation invocation)
        {
            return invocation.Method.GetDisplayName(true, invocation.Arguments.HasValue() ? invocation.Arguments.Select(x => x == null ? "null" :  _typeConverter != null ? _typeConverter.ConvertTo<string>(x) : x.ToString()).ToArray() : null);
        }

        private ILogger GetLoggerFor(IInvocation invocation)
        {
            if (_factory != null)
            {
                return _factory.CreateLogger(invocation.TargetType);
            }

            return _logger;
        }
    }
}
