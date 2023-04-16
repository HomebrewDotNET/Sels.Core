using Sels.ObjectValidationFramework.Rules;
using System;
using static Sels.Core.Delegates.Async;

namespace Sels.ObjectValidationFramework.Configurators
{

    /// <summary>
    /// Configurator for creating validation rules for value of type <typeparamref name="TValue"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
    /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
    /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    public interface IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> : IValidationConfigurator<TEntity, TBaseContext, TError> where TTargetContext : TBaseContext
    {
        /// <summary>
        /// Creates a new validation rule that validates values of type <typeparamref name="TValue"/> using <paramref name="condition"/>. <paramref name="errorConstructor"/> will create a validation error of type <typeparamref name="TError"/> when <paramref name="condition"/> fails.
        /// </summary>
        /// <param name="condition">Delegate that checks if <see cref="IValidationRuleContext{TEntity, TInfo, TTargetContext, TValue}.Value"/> is a valid value</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TTargetContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator for method chaining</returns>
        IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> ValidIf(Predicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor);

        /// <summary>
        /// Creates a new validation rule that validates values of type <typeparamref name="TValue"/> using <paramref name="condition"/>. <paramref name="errorConstructor"/> will create a validation error of type <typeparamref name="TError"/> when <paramref name="condition"/> succeeds.
        /// </summary>
        /// <param name="condition">Delegate that checks if <see cref="IValidationRuleContext{TEntity, TInfo, TTargetContext, TValue}.Value"/> is a valid value</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TTargetContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator for method chaining</returns>
        IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> InvalidIf(Predicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor);

        /// <summary>
        /// Validation rules configured after this method call will only be performed when <paramref name="condition"/> returns true.
        /// </summary>
        /// <param name="condition">Delegate that checks if the validation rules configured after this method call can be executed</param>
        /// <returns>Current configurator for method chaining</returns>
        IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> NextWhen(Predicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition);

        #region Async
        /// <summary>
        /// Creates a new validation rule that validates values of type <typeparamref name="TValue"/> using <paramref name="condition"/>. <paramref name="errorConstructor"/> will create a validation error of type <typeparamref name="TError"/> when <paramref name="condition"/> fails.
        /// </summary>
        /// <param name="condition">Delegate that checks if <see cref="IValidationRuleContext{TEntity, TInfo, TTargetContext, TValue}.Value"/> is a valid value</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TTargetContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator for method chaining</returns>
        IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> ValidIf(AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition, AsyncFunc<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor);

        /// <summary>
        /// Creates a new validation rule that validates values of type <typeparamref name="TValue"/> using <paramref name="condition"/>. <paramref name="errorConstructor"/> will create a validation error of type <typeparamref name="TError"/> when <paramref name="condition"/> succeeds.
        /// </summary>
        /// <param name="condition">Delegate that checks if <see cref="IValidationRuleContext{TEntity, TInfo, TTargetContext, TValue}.Value"/> is a valid value</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TTargetContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator for method chaining</returns>
        IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> InvalidIf(AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition, AsyncFunc<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor);

        /// <summary>
        /// Validation rules configured after this method call will only be performed when <paramref name="condition"/> returns true.
        /// </summary>
        /// <param name="condition">Delegate that checks if the validation rules configured after this method call can be executed</param>
        /// <returns>Current configurator for method chaining</returns>
        IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> NextWhen(AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition);
        #endregion
    }

    /// <summary>
    /// Configurator for creating validation rules for value of type <typeparamref name="TValue"/> using an untyped context.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
    /// <typeparam name="TContext">Type of the validation context used by the current validator</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    public interface IValidationTargetRootConfigurator<TEntity, TError, TContext, TInfo, TValue> : IValidationTargetConfigurator<TEntity, TError, TContext, TInfo, TContext, TValue>
    {
        /// <summary>
        /// Selects a new context type for the current validation target.
        /// </summary>
        /// <returns>Current configurator for method chaining with a context of type <typeparamref name="TContext"/></returns>
        IValidationTargetConfigurator<TEntity, TError, TContext, TInfo, TNewContext, TValue> WithContext<TNewContext>() where TNewContext : TContext;
    }
}
