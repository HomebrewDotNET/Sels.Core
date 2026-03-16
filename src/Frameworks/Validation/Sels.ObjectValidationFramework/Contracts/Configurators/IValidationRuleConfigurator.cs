using Sels.ObjectValidationFramework.Rules;
using System;
using System.Collections.Generic;
using System.Text;
using static Sels.Core.Delegates.Async;

namespace Sels.ObjectValidationFramework.Configurators
{
    /// <summary>
    /// Configurator for configuring the current validation rule.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
    /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
    /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    public interface IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> : IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> where TTargetContext : TBaseContext
    {
        /// <summary>
        /// The current validation rule will only be executed when <paramref name="condition"/> returns true.
        /// </summary>
        /// <param name="condition">Delegate that checks if the current rule can be executed</param>
        /// <returns>Current configurator for method chaining</returns>
        IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> When(Predicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition);

        #region Async
        /// <summary>
        /// The current validation rule will only be executed when <paramref name="condition"/> returns true.
        /// </summary>
        /// <param name="condition">Delegate that checks if the current rule can be executed</param>
        /// <returns>Current configurator for method chaining</returns>
        IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> When(AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition);
        #endregion
    }
}
