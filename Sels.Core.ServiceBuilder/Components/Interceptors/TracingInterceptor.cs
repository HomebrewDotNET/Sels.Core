using Castle.DynamicProxy;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Models.Disposables;
using Sels.Core.ServiceBuilder.Template.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Extensions.Logging;
using Microsoft.Extensions.Logging;

namespace Sels.Core.ServiceBuilder.Interceptors
{
    /// <summary>
    /// Interceptor that traces method duration and/or exceptions.
    /// </summary>
    public class TracingInterceptor : BaseResultlessInterceptor, ITracingInterceptorBuilder, IMethodDurationInterceptorBuilder
    {
        // Fields
        private readonly ILoggerFactory? _factory;
        private readonly IEnumerable<ILogger?>? _loggers;

        // State
        private MethodTracer? _methodTracer;
        private ExceptionTracer? _exceptionTracer;

        /// <inheritdoc cref="TracingInterceptor"/>
        /// <param name="loggers">Static loggers to use for tracing</param>
        public TracingInterceptor(IEnumerable<ILogger?>? loggers)
        {
            _loggers = loggers != null ? loggers.Where(x => x != null) : null;
        }

        /// <inheritdoc cref="TracingInterceptor"/>
        /// <param name="factory">Logger factory for creating loggers based on the target type</param>
        public TracingInterceptor(ILoggerFactory? factory)
        {
            _factory = factory;
        }
        /// <inheritdoc/>
        public IMethodDurationInterceptorBuilder Duration => this;
        /// <inheritdoc/>
        public IExceptionTracingInterceptorBuilder Exceptions { get { _exceptionTracer = new ExceptionTracer(this); return _exceptionTracer; } }
        /// <inheritdoc/>
        public IAllMethodDurationInterceptorBuilder OfAll { get { var tracer = new AllMethodTracer(this); _methodTracer = tracer; return tracer; } }
        /// <inheritdoc/>
        public ISpecificMethodDurationInterceptorBuilder Of { get { var tracer = new SpecificMethodTracer(this); _methodTracer = tracer; return tracer; } }

        /// <inheritdoc/>
        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            var loggers = GetLoggersFor(invocation);

            try
            {
                using (_methodTracer != null && _methodTracer.CanTrace(invocation) ? loggers.TraceMethod(invocation.MethodInvocationTarget.ReflectedType, invocation.MethodInvocationTarget.Name) : NullDisposer.Instance)
                {
                    await proceed(invocation, proceedInfo);
                }               
            }
            catch (Exception ex)
            {
                if(loggers != null)
                {
                    _exceptionTracer?.Trace(invocation, loggers, ex);
                }

                throw;
            }
 
        }

        private IEnumerable<ILogger> GetLoggersFor(IInvocation invocation)
        {
            if (_factory != null)
            {
                return _factory.CreateLogger(invocation.Proxy.GetType()).AsEnumerable();
            }

            return _loggers;
        }

        #region Helper classes
        private class ExceptionTracer : Delegator, IExceptionTracingInterceptorBuilder
        {
            // Fields
            private List<Predicate<Exception>> _conditions = new();
            private Func<Exception, LogLevel?>? _logLevelSelector;
            private Action<IInvocation, IEnumerable<ILogger>, LogLevel, Exception>? _logger;

            public ExceptionTracer(ITracingInterceptorBuilder builder) : base(builder)
            {
            }

            public void Trace(IInvocation invocation, IEnumerable<ILogger> loggers, Exception exception)
            {
                if (_conditions.Count == 0 || _conditions.Any(x => x(exception)))
                {
                    var logLevel = _logLevelSelector?.Invoke(exception) ?? LogLevel.Error;

                    if (_logger != null) _logger(invocation, loggers, logLevel, exception); else loggers.LogException(logLevel, exception);
                } 
            }

            public IExceptionTracingInterceptorBuilder Using(Action<IInvocation, IEnumerable<ILogger>, LogLevel, Exception> logger)
            {
               _logger = logger.ValidateArgument(nameof(logger));
                return this;
            }

            public IExceptionTracingInterceptorBuilder When(Predicate<Exception> condition)
            {
                condition.ValidateArgument(nameof(condition));

                _conditions.Add(condition);
                return this;
            }

            public IExceptionTracingInterceptorBuilder WithLevel(Func<Exception, LogLevel?> selector)
            {
                _logLevelSelector = selector.ValidateArgument(nameof(selector));
                return this;
            }
        }
        private class AllMethodTracer : MethodTracer, IAllMethodDurationInterceptorBuilder
        {
            // Fields
            private readonly List<Predicate<IInvocation>> _exceptions = new List<Predicate<IInvocation>>();

            public AllMethodTracer(ITracingInterceptorBuilder builder) : base(builder)
            {
            }

            public override bool CanTrace(IInvocation invocation)
            {
                return _exceptions.Count == 0 || !_exceptions.Any(x => x(invocation));
            }

            public IAllMethodDurationInterceptorBuilder Except(Predicate<IInvocation> condition)
            {
                condition.ValidateArgument(nameof(condition));

                _exceptions.Add(condition);
                return this;
            }
        }
        private class SpecificMethodTracer : MethodTracer, ISpecificMethodDurationInterceptorBuilder
        {
            // Fields
            private readonly List<Predicate<IInvocation>> _selectors = new List<Predicate<IInvocation>>();

            public SpecificMethodTracer(ITracingInterceptorBuilder builder) : base(builder)
            {
            }

            public override bool CanTrace(IInvocation invocation)
            {
                return _selectors.Count != 0 && _selectors.Any(x => x(invocation));
            }

            public ISpecificMethodDurationInterceptorBuilder Methods(Predicate<IInvocation> condition)
            {
                condition.ValidateArgument(nameof(condition));

                _selectors.Add(condition);
                return this;
            }
        }
        private abstract class MethodTracer : Delegator
        {
            protected MethodTracer(ITracingInterceptorBuilder builder) : base(builder)
            {
            }

            public abstract bool CanTrace(IInvocation invocation);
        }
        private abstract class Delegator : ITracingInterceptorBuilder
        {
            // Fields
            private readonly ITracingInterceptorBuilder _builder;

            public Delegator(ITracingInterceptorBuilder builder)
            {
                _builder = builder.ValidateArgument(nameof(builder));
            }

            public IMethodDurationInterceptorBuilder Duration => _builder.Duration;

            public IExceptionTracingInterceptorBuilder Exceptions => _builder.Exceptions;
        }
        #endregion
    }
}
