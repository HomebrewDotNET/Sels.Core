using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions.Reflection;
using Microsoft.Extensions.Logging;

namespace Sels.Core.ServiceBuilder.Interceptors
{
    /// <summary>
    /// Builder for creating an interceptor that traces method duration and/or exceptions.
    /// </summary>
    public interface ITracingInterceptorBuilder
    {
        /// <summary>
        /// Traces the duration of method execution.
        /// </summary>
        IMethodDurationInterceptorBuilder Duration { get; }
        /// <summary>
        /// Traces any thrown exceptions.
        /// </summary>
        IExceptionTracingInterceptorBuilder Exceptions { get; }
    }

    /// <summary>
    /// Builder for creating an interceptor that traces method duration.
    /// </summary>
    public interface IMethodDurationInterceptorBuilder
    {
        /// <summary>
        /// Traces the duration of all methods.
        /// </summary>
        IAllMethodDurationInterceptorBuilder OfAll { get; }
        /// <summary>
        /// Traces the duration of specific methods.
        /// </summary>
        ISpecificMethodDurationInterceptorBuilder Of { get; }
    }
    /// <summary>
    /// Builder for creating an interceptor that traces all method duration.
    /// </summary>
    public interface IAllMethodDurationInterceptorBuilder : ITracingInterceptorBuilder
    {
        /// <summary>
        /// All methods matching <paramref name="condition"/> will not be traced.
        /// </summary>
        /// <param name="condition">Delegate that dictates if a method should not be traced</param>
        /// <returns>Current builder for method chaining</returns>
        IAllMethodDurationInterceptorBuilder Except(Predicate<IInvocation> condition);
        /// <summary>
        /// <paramref name="method"/> will not be traced.
        /// </summary>
        /// <param name="method">The method not to trace</param>
        /// <returns>Current builder for method chaining</returns>
        IAllMethodDurationInterceptorBuilder ExceptMethod(MethodInfo method) => Except(x => x.Method.AreEqual(method.ValidateArgument(nameof(method))));
        /// <summary>
        /// Methods with <paramref name="methodname"/> will not be traced.
        /// </summary>
        /// <param name="methodname">The name of the methods not to trace</param>
        /// <returns>Current builder for method chaining</returns>
        IAllMethodDurationInterceptorBuilder ExceptMethod(string methodname) => Except(x => x.Method.Name.Equals(methodname.ValidateArgumentNotNullOrWhitespace(methodname)));
        /// <summary>
        /// Methods with <paramref name="methodNames"/> will not be traced.
        /// </summary>
        /// <param name="methodNames">The names of the methods not to trace</param>
        /// <returns>Current builder for method chaining</returns>
        IAllMethodDurationInterceptorBuilder ExceptMethods(params string[] methodNames) => Except(x => methodNames.ValidateArgument(nameof(methodNames)).Contains(x.Method.Name));
    }
    /// <summary>
    /// Builder for creating an interceptor that traces the duration of specific methods.
    /// </summary>
    public interface ISpecificMethodDurationInterceptorBuilder : ITracingInterceptorBuilder
    {
        /// <summary>
        /// All methods matching <paramref name="condition"/> will be traced.
        /// </summary>
        /// <param name="condition">Delegate that dictates if a method should be traced</param>
        /// <returns>Current builder for method tracing</returns>
        ISpecificMethodDurationInterceptorBuilder Methods(Predicate<IInvocation> condition);
        /// <summary>
        /// <paramref name="method"/> will be traced.
        /// </summary>
        /// <param name="method">The method to trace</param>
        /// <returns>Current builder for method chaining</returns>
        ISpecificMethodDurationInterceptorBuilder Method(MethodInfo method) => Methods(x => x.Method.AreEqual(method.ValidateArgument(nameof(method))));
        /// <summary>
        /// Methods with <paramref name="methodname"/> will be traced.
        /// </summary>
        /// <param name="methodname">The name of the methods not to trace</param>
        /// <returns>Current builder for method chaining</returns>
        ISpecificMethodDurationInterceptorBuilder Method(string methodname) => Methods(x => x.Method.Name.Equals(methodname.ValidateArgumentNotNullOrWhitespace(methodname)));
        /// <summary>
        /// Methods with <paramref name="methodNames"/> will be traced.
        /// </summary>
        /// <param name="methodNames">The names of the methods to trace</param>
        /// <returns>Current builder for method chaining</returns>
        ISpecificMethodDurationInterceptorBuilder Methods(params string[] methodNames) => Methods(x => methodNames.ValidateArgument(nameof(methodNames)).Contains(x.Method.Name));
    }

    /// <summary>
    /// Builder for creating an interceptor that traces any exception thrown.
    /// </summary>
    public interface IExceptionTracingInterceptorBuilder : ITracingInterceptorBuilder
    {
        /// <summary>
        /// Defines when to trace exception. Default is all exceptions if no condition is set.
        /// </summary>
        /// <param name="condition">Delegate that dictates when to trace an exception</param>
        /// <returns>Current builder for method tracing</returns>
        IExceptionTracingInterceptorBuilder When(Predicate<Exception> condition);
        /// <summary>
        /// Traces exception assignable to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the exception to trace</typeparam>
        /// <returns>Current builder for method tracing</returns>
        IExceptionTracingInterceptorBuilder OfType<T>() where T : Exception => When(x => x.IsAssignableTo<T>());
        /// <summary>
        /// Defines the log level to use for specific exceptions. If no delegates are set the default is <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="selector">Delegate that returns the log level for the provided exception. If null is returned then the default log level is used</param>
        /// <returns>Current builder for method tracing</returns>
        IExceptionTracingInterceptorBuilder WithLevel(Func<Exception, LogLevel?> selector);
        /// <summary>
        /// Defines a custom delegate for logging the exception.
        /// </summary>
        /// <param name="logger">Delegate that logs the exceptions using the provided loggers</param>
        /// <returns>Current builder for method tracing</returns>
        IExceptionTracingInterceptorBuilder Using(Action<IInvocation, IEnumerable<ILogger>, LogLevel, Exception> logger);
    }
}
