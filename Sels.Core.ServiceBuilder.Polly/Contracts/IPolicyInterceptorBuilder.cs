using Polly;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.ServiceBuilder.Polly
{
    /// <summary>
    /// Builder for creating an interceptor to execute methods with Polly policies.
    /// Builder selects the target methods to intercept.
    /// </summary>
    /// <typeparam name="T">The target type to intercept</typeparam>
    public interface IRootPolicyInterceptorBuilder<T>
    {
        /// <summary>
        /// Executes <paramref name="method"/> with a Polly policy.
        /// </summary>
        /// <param name="method">The method to intercept</param>
        /// <returns>Builder for selecting the policy</returns>
        ISelectSyncPolicyInterceptorBuilder<T> For(MethodInfo method);
        /// <summary>
        /// Executes <paramref name="method"/> with an async Polly policy.
        /// </summary>
        /// <param name="method">The method to intercept</param>
        /// <returns>Builder for selecting the policy</returns>
        ISelectAsyncPolicyInterceptorBuilder<T> ForAsync(MethodInfo method);
        /// <summary>
        /// Executes the method selected by <paramref name="methodExpression"/> with a Polly policy.
        /// </summary>
        /// <param name="methodExpression">Expression that points to the method to intercept</param>
        /// <returns>Builder for selecting the policy</returns>
        ISelectSyncPolicyInterceptorBuilder<T> For(Expression<Func<T, object>> methodExpression) => For(methodExpression.ExtractMethod(nameof(methodExpression)));
        /// <summary>
        /// Executes the method selected by <paramref name="methodExpression"/> with a Polly policy.
        /// </summary>
        /// <param name="methodExpression">Expression that points to the method to intercept</param>
        /// <returns>Builder for selecting the policy</returns>
        ISelectSyncPolicyInterceptorBuilder<T> For(Expression<Action<T>> methodExpression) => For(methodExpression.ExtractMethod(nameof(methodExpression)));
        /// <summary>
        /// Executes the method selected by <paramref name="methodExpression"/> with a Polly policy.
        /// </summary>
        /// <param name="methodExpression">Expression that points to the method to intercept</param>
        /// <returns>Builder for selecting the policy</returns>
        ISelectAsyncPolicyInterceptorBuilder<T> ForAsync(Expression<Func<T, Task>> methodExpression) => ForAsync(methodExpression.ExtractMethod(nameof(methodExpression)));
        /// <summary>
        /// Executes the method selected by <paramref name="methodExpression"/> with am async Polly policy.
        /// </summary>
        /// <param name="methodExpression">Expression that points to the method to intercept</param>
        /// <returns>Builder for selecting the policy</returns>
        ISelectAsyncPolicyInterceptorBuilder<T> ForAsync(Expression<Func<T, ValueTask>> methodExpression) => ForAsync(methodExpression.ExtractMethod(nameof(methodExpression)));
        /// <summary>
        /// Executes all synchronous methods with a Polly policy.
        /// </summary>
        ISelectSyncPolicyInterceptorBuilder<T> ForAll { get; }
        /// <summary>
        /// Executes all asynchronous methods with am async Polly policy.
        /// </summary>
        ISelectAsyncPolicyInterceptorBuilder<T> ForAllAsync { get; }
    }

    /// <summary>
    /// Builder for creating an interceptor to execute methods with Polly policies.
    /// Builder selects the policies to use.
    /// </summary>
    /// <typeparam name="T">The target type to intercept</typeparam>
    public interface ISelectSyncPolicyInterceptorBuilder<T>
    {
        /// <summary>
        /// Defines a policy to execute the current target with.
        /// Multiple policies can be added for the same target.
        /// </summary>
        /// <param name="policy">The policy to use</param>
        /// <param name="order">The execution order of the policy. When multiple policies are defined for the same target they are ordered ascending to get the execution order</param>
        /// <returns>Builder for configuring more targets or to configure the current target</returns>
        ISyncPolicyInterceptorBuilder<T> ExecuteWith(ISyncPolicy policy, uint order = 0);
    }

    /// <summary>
    /// Builder for creating an interceptor to execute methods with Polly policies.
    /// Builder selects more targets to intercept or configure the current target.
    /// </summary>
    /// <typeparam name="T">The target type to intercept</typeparam>
    public interface ISyncPolicyInterceptorBuilder<T> : IRootPolicyInterceptorBuilder<T>, ISelectSyncPolicyInterceptorBuilder<T>
    {

    }

    /// <summary>
    /// Builder for creating an interceptor to execute methods with Polly policies.
    /// Builder selects the policies to use.
    /// </summary>
    /// <typeparam name="T">The target type to intercept</typeparam>
    public interface ISelectAsyncPolicyInterceptorBuilder<T>
    {
        /// <summary>
        /// Defines a policy to execute the current target with.
        /// Multiple policies can be added for the same target.
        /// </summary>
        /// <param name="policy">The policy to use</param>
        /// <param name="order">The execution order of the policy. When multiple policies are defined for the same target they are ordered ascending to get the execution order</param>
        /// <returns>Builder for configuring more targets or to configure the current target</returns>
        IAsyncPolicyInterceptorBuilder<T> ExecuteWith(IAsyncPolicy policy, uint order = 0);
    }

    /// <summary>
    /// Builder for creating an interceptor to execute methods with Polly policies.
    /// Builder selects more targets to intercept or configure the current target.
    /// </summary>
    /// <typeparam name="T">The target type to intercept</typeparam>
    public interface IAsyncPolicyInterceptorBuilder<T> : IRootPolicyInterceptorBuilder<T>, ISelectAsyncPolicyInterceptorBuilder<T>
    {

    }
}
