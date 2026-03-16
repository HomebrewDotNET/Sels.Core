using Sels.ObjectValidationFramework.Rules;
using System;
using System.Collections.Generic;
using System.Text;
using static Sels.Core.Delegates.Async;

namespace Sels.ObjectValidationFramework.Configurators
{
    /// <summary>
    /// Builder for creating conditions using a switch like syntax.
    /// </summary>
    /// <typeparam name="TEntity">Type of object to create validation rules for</typeparam>
    /// <typeparam name="TBaseContext">The type of the context used by the parent validator</typeparam>
    /// <typeparam name="TTargetContext">Type of the validation context used by all rules</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TValue">Type of the value to switch on</typeparam>
    public interface ISwitchRootConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue>
    {
        #region Case
        /// <summary>
        /// Returns a builder for creating validation rules when <paramref name="condition"/> returns true.
        /// </summary>
        /// <param name="condition">Delegate that checks if the value satisfies the current case</param>
        /// <returns>Current builder for method chaining</returns>
        ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue> Case(AsyncPredicate<TValue> condition);
        /// <summary>
        /// Returns a builder for creating validation rules when <paramref name="condition"/> returns true.
        /// </summary>
        /// <param name="condition">Delegate that checks if the value satisfies the current case</param>
        /// <returns>Current builder for method chaining</returns>
        ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue> Case(Predicate<TValue> condition);
        /// <summary>
        /// Returns a builder for creating validation rules when the switch value is equal to <paramref name="match"/>.
        /// </summary>
        /// <param name="match">The value that the switch value must be equal to</param>
        /// <returns>Current builder for method chaining</returns>
        ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue> Case(TValue match);
        #endregion
    }

    /// <summary>
    /// Builder for defining more switch conditions or build conditions using the currently defined cases.
    /// </summary>
    /// <typeparam name="TEntity">Type of object to create validation rules for</typeparam>
    /// <typeparam name="TBaseContext">The type of the context used by the parent validator</typeparam>
    /// <typeparam name="TTargetContext">Type of the validation context used by all rules</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TValue">Type of the value to switch on</typeparam>
    public interface ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue>
    {
        #region Case
        /// <summary>
        /// Returns a builder for creating validation rules when <paramref name="condition"/> returns true or when any previously defined case returns true.
        /// </summary>
        /// <param name="condition">Delegate that checks if the value satisfies the current case</param>
        /// <returns>Current builder for method chaining</returns>
        ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue> Case(AsyncPredicate<TValue> condition);
        /// <summary>
        /// Returns a builder for creating validation rules when <paramref name="condition"/> returns true or when any previously defined case returns true.
        /// </summary>
        /// <param name="condition">Delegate that checks if the value satisfies the current case</param>
        /// <returns>Current builder for method chaining</returns>
        ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue> Case(Predicate<TValue> condition);
        /// <summary>
        /// Returns a builder for creating validation rules when the switch value is equal to <paramref name="match"/> or when any previously defined case returns true.
        /// </summary>
        /// <param name="match">The value that the switch value must be equal to</param>
        /// <returns>Current builder for method chaining</returns>
        ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue> Case(TValue match);
        #endregion

        /// <summary>
        /// Create validation rules using <paramref name="builder"/> that will only be executed when any of the previously defined cases returns true.
        /// </summary>
        /// <param name="builder">Delegate used to create validation rules</param>
        /// <returns>Current builder for method chaining</returns>
        ISwitchFullConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue> Then(Action<IValidationConfigurator<TEntity, TTargetContext, TError>> builder);
    }

    /// <summary>
    /// Builder for defining more switch cases, set a default when none of the switch cases match or exit the current builder.
    /// </summary>
    /// <typeparam name="TEntity">Type of object to create validation rules for</typeparam>
    /// <typeparam name="TBaseContext">The type of the context used by the parent validator</typeparam>
    /// <typeparam name="TTargetContext">Type of the validation context used by all rules</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TValue">Type of the value to switch on</typeparam>
    public interface ISwitchFullConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue> : ISwitchRootConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue>, IValidationConfigurator<TEntity, TBaseContext, TError>
    {
        /// <summary>
        /// Create validation rules using <paramref name="builder"/> when none of the previously defined cases are matched.
        /// </summary>
        /// <param name="builder">Delegate used to create validation rules</param>
        /// <returns>Current builder for method chaining</returns>
        IValidationConfigurator<TEntity, TBaseContext, TError> Default(Action<IValidationConfigurator<TEntity, TTargetContext, TError>> builder);
    }
}
