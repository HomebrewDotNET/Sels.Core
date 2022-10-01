using Sels.ObjectValidationFramework.Contracts.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using static Sels.Core.Delegates.Async;

namespace Sels.ObjectValidationFramework.Contracts.Rules
{

    /// <summary>
    /// Configurator for creating validation rules for value of type <typeparamref name="TValue"/> using a context of type <typeparamref name="TContext"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
    /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    public interface IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> : IValidationConfigurator<TEntity, TError>
    {
        /// <summary>
        /// Creates a new validation rule that validates values of type <typeparamref name="TValue"/> using <paramref name="condition"/>. <paramref name="errorConstructor"/> will create a validation error of type <typeparamref name="TError"/> when <paramref name="condition"/> fails.
        /// </summary>
        /// <param name="condition">Delegate that checks if <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is a valid value</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> ValidIf(Predicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>> condition, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor);

        /// <summary>
        /// Creates a new validation rule that validates values of type <typeparamref name="TValue"/> using <paramref name="condition"/>. <paramref name="errorConstructor"/> will create a validation error of type <typeparamref name="TError"/> when <paramref name="condition"/> succeeds.
        /// </summary>
        /// <param name="condition">Delegate that checks if <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is a valid value</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> InvalidIf(Predicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>> condition, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor);

        /// <summary>
        /// Validation rules configured after this method call will only be performed when <paramref name="condition"/> returns true.
        /// </summary>
        /// <param name="condition">Delegate that checks if the validation rules configured after this method call can be executed</param>
        /// <returns>Current configurator</returns>
        IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> ValidateWhen(Predicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>> condition);

        #region Async
        /// <summary>
        /// Creates a new validation rule that validates values of type <typeparamref name="TValue"/> using <paramref name="condition"/>. <paramref name="errorConstructor"/> will create a validation error of type <typeparamref name="TError"/> when <paramref name="condition"/> fails.
        /// </summary>
        /// <param name="condition">Delegate that checks if <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is a valid value</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> ValidIf(AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>> condition, AsyncFunc<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor);

        /// <summary>
        /// Creates a new validation rule that validates values of type <typeparamref name="TValue"/> using <paramref name="condition"/>. <paramref name="errorConstructor"/> will create a validation error of type <typeparamref name="TError"/> when <paramref name="condition"/> succeeds.
        /// </summary>
        /// <param name="condition">Delegate that checks if <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is a valid value</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> InvalidIf(AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>> condition, AsyncFunc<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor);

        /// <summary>
        /// Validation rules configured after this method call will only be performed when <paramref name="condition"/> returns true.
        /// </summary>
        /// <param name="condition">Delegate that checks if the validation rules configured after this method call can be executed</param>
        /// <returns>Current configurator</returns>
        IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> ValidateWhen(AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>> condition);
        #endregion
    }

    /// <summary>
    /// Configurator for creating validation rules for value of type <typeparamref name="TValue"/> using an untyped context.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    public interface IValidationRuleConfigurator<TEntity, TError, TInfo, TValue> : IValidationRuleConfigurator<TEntity, TError, TInfo, object, TValue>
    {
        /// <summary>
        /// Creates a configurator that uses a typed context of type <typeparamref name="TContext"/> used to create validatiion rules.
        /// </summary>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="required">If the context is required to be supplied. When set to true <see cref="IValidationRuleContext{TEntity, TContext}.WasContextSupplied"/> must be set to true for the rules to trigger</param>
        /// <returns>Current configurator with a context of type <typeparamref name="TContext"/></returns>
        IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> WithContext<TContext>(bool required = false);
    }
}
