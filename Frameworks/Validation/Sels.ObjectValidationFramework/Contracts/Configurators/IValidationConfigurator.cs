using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Sels.ObjectValidationFramework.Rules;
using Sels.ObjectValidationFramework.Target;
using static Sels.Core.Delegates.Async;

namespace Sels.ObjectValidationFramework.Configurators
{
    /// <summary>
    /// Configurator for creating validation rules on object of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of object to create validation rules for</typeparam>
    /// <typeparam name="TContext">Type of the validation context used by all rules</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    public interface IValidationConfigurator<TEntity, TContext, TError>
    {
        #region Target
        /// <summary>
        /// Creates a validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="settings">Extra settings for the rule</param>
        /// <returns>Validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/></returns>
        IValidationTargetRootConfigurator<TEntity, TError, TContext, NullValidationInfo, TEntity> ForSource(TargetExecutionOptions settings = TargetExecutionOptions.None);

        /// <summary>
        /// Creates validation target configurator for creating validation rules for <typeparamref name="TValue"/> selected from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="valueSelector">Selects the value to validate from an instance of <typeparamref name="TEntity"/></param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <typeparam name="TValue">Type of value to validate</typeparam>
        /// <returns>Validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/></returns>
        IValidationTargetRootConfigurator<TEntity, TError, TContext, NullValidationInfo, TValue> ForSource<TValue>(Func<TEntity, TValue> valueSelector, TargetExecutionOptions settings = TargetExecutionOptions.None);

        /// <summary>
        /// Creates validation target configurator for creating validation rules for <paramref name="property"/> on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">Delegate that selects which property to validate. Will throw an <see cref="ArgumentException"/> when anything but a property is selected</param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <typeparam name="TPropertyValue">Type of property to validate</typeparam>
        /// <returns>Validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/></returns>
        IValidationTargetRootConfigurator<TEntity, TError, TContext, PropertyValidationInfo, TPropertyValue> ForProperty<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> property, TargetExecutionOptions settings = TargetExecutionOptions.None);

        /// <summary>
        /// Creates validation target configurator for creating validation rules for the value selected by <paramref name="valueSelector"/> from <paramref name="property"/> on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">Delegate that selects which property to validate. Will throw an <see cref="ArgumentException"/> when anything but a property is selected</param>
        /// <param name="valueSelector">Delegate that selects the value to validate on <paramref name="property"/></param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <typeparam name="TPropertyValue">Type of property to validate</typeparam>
        /// <typeparam name="TValue">Type of value to validate</typeparam>
        /// <returns>Validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/></returns>
        IValidationTargetRootConfigurator<TEntity, TError, TContext, PropertyValidationInfo, TValue> ForProperty<TPropertyValue, TValue>(Expression<Func<TEntity, TPropertyValue>> property, Func<TPropertyValue, TValue> valueSelector, TargetExecutionOptions settings = TargetExecutionOptions.None);

        /// <summary>
        /// Creates validation target configurator for creating validation rules for the elements in <paramref name="property"/> on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">Delegate that selects which property to validate. Will throw an <see cref="ArgumentException"/> when anything but a property is selected</param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <typeparam name="TElement">Type of element to validate</typeparam>
        /// <returns>Validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/></returns>
        IValidationTargetRootConfigurator<TEntity, TError, TContext, CollectionPropertyValidationInfo, TElement> ForElements<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> property, TargetExecutionOptions settings = TargetExecutionOptions.None);

        /// <summary>
        /// Creates validation target configurator for creating validation rules for the value selected by <paramref name="valueSelector"/> for each element in <paramref name="property"/> on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">Delegate that selects which property to validate. Will throw an <see cref="ArgumentException"/> when anything but a property is selected</param>
        /// <param name="valueSelector">Delegate that selects the value to validate on each element in <paramref name="property"/></param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <typeparam name="TElement">Type of element to validate</typeparam>
        /// <typeparam name="TValue">Type of value to validate</typeparam>
        /// <returns>Validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/></returns>
        IValidationTargetRootConfigurator<TEntity, TError, TContext, CollectionPropertyValidationInfo, TValue> ForElements<TElement, TValue>(Expression<Func<TEntity, IEnumerable<TElement>>> property, Func<TElement, TValue> valueSelector, TargetExecutionOptions settings = TargetExecutionOptions.None);

        #endregion

        #region Condition
        /// <summary>
        /// All validation rules created with <paramref name="configurator"/> will only be executed when <paramref name="condition"/> passes.
        /// </summary>
        /// <param name="condition">Delegate that checks if validation rules created with <paramref name="configurator"/> can be executed</param>
        /// <param name="configurator">Validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/></param>
        /// <returns>Validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/></returns>
        IValidationConfigurator<TEntity, TContext, TError> ValidateWhen(Predicate<IValidationRuleContext<TEntity, TContext>> condition, Action<IValidationConfigurator<TEntity, TContext, TError>> configurator);
        /// <summary>
        /// All validation rules created with <paramref name="configurator"/> will only be executed when <paramref name="condition"/> passes. Condition uses a context of type <typeparamref name="TNewContext"/>.
        /// </summary>
        /// <typeparam name="TNewContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="condition">Delegate that checks if validation rules created with <paramref name="configurator"/> can be executed</param>
        /// <param name="configurator">Validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/></param>
        /// <param name="contextRequired">
        /// If a context of <typeparamref name="TNewContext"/> is required for the validation rules created with <paramref name="configurator"/>. 
        /// If set to false and context is not of type <typeparamref name="TNewContext"/> then <see cref="IValidationRuleContext{TEntity, TContext}.HasContext"/> will be set to false.
        /// When set to true the rules will only be executed when the supplied context is of type <typeparamref name="TNewContext"/>.
        /// </param>
        /// <returns>Validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/></returns>
        IValidationConfigurator<TEntity, TContext, TError> ValidateWhen<TNewContext>(Predicate<IValidationRuleContext<TEntity, TNewContext>> condition, Action<IValidationConfigurator<TEntity, TNewContext, TError>> configurator, bool contextRequired = true) where TNewContext : TContext;

        /// <summary>
        /// All validation rules created with <paramref name="configurator"/> will only be executed when <paramref name="condition"/> passes.
        /// </summary>
        /// <param name="condition">Delegate that checks if validation rules created with <paramref name="configurator"/> can be executed</param>
        /// <param name="configurator">Validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/></param>
        /// <returns>Validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/></returns>
        IValidationConfigurator<TEntity, TContext, TError> ValidateWhen(AsyncPredicate<IValidationRuleContext<TEntity, TContext>> condition, Action<IValidationConfigurator<TEntity, TContext, TError>> configurator);
        /// <summary>
        /// All validation rules created with <paramref name="configurator"/> will only be executed when <paramref name="condition"/> passes. Condition uses a context of type <typeparamref name="TNewContext"/>.
        /// </summary>
        /// <typeparam name="TNewContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="condition">Delegate that checks if validation rules created with <paramref name="configurator"/> can be executed</param>
        /// <param name="configurator">Validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/></param>
        /// <param name="contextRequired">
        /// If a context of <typeparamref name="TNewContext"/> is required for the validation rules created with <paramref name="configurator"/>. 
        /// If set to false and context is not of type <typeparamref name="TNewContext"/> then <see cref="IValidationRuleContext{TEntity, TContext}.HasContext"/> will be set to false.
        /// When set to true the rules will only be executed when the supplied context is of type <typeparamref name="TNewContext"/>.
        /// </param>
        /// <returns>Validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/></returns>
        IValidationConfigurator<TEntity, TContext, TError> ValidateWhen<TNewContext>(AsyncPredicate<IValidationRuleContext<TEntity, TNewContext>> condition, Action<IValidationConfigurator<TEntity, TNewContext, TError>> configurator, bool contextRequired = true) where TNewContext : TContext;

        /// <summary>
        /// All validation rules created after calling this method will only be executed when <paramref name="condition"/> passes.
        /// </summary>
        /// <param name="condition">Delegate that checks if validation rules created after calling this method can be executed</param>
        /// <returns>Validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/></returns>
        IValidationConfigurator<TEntity, TContext, TError> ValidateNextWhen(AsyncPredicate<IValidationRuleContext<TEntity, TContext>> condition);

        /// <summary>
        /// All validation rules created after calling this method will only be executed when <paramref name="condition"/> passes.
        /// </summary>
        /// <param name="condition">Delegate that checks if validation rules created after calling this method can be executed</param>
        /// <returns>Validation target configurator for creating validation rules on the selected value from <typeparamref name="TEntity"/></returns>
        IValidationConfigurator<TEntity, TContext, TError> ValidateNextWhen(Predicate<IValidationRuleContext<TEntity, TContext>> condition);

        /// <summary>
        /// Returns a builder for creating validation conditions using a switch like syntax.
        /// </summary>
        /// <typeparam name="TValue">Type of the value to switch on</typeparam>
        /// <param name="valueSelector">Delegate that selects the value to switch on</param>
        /// <param name="condition">Optional condition that dictates if the current switch statemant can be executed</param>
        /// <returns>Builder for creating validation conditions using a switch like syntax</returns>
        ISwitchRootConditionConfigurator<TEntity, TError, TContext, TContext, TValue> Switch<TValue>(Func<TEntity, TValue> valueSelector, Predicate<IValidationRuleContext<TEntity, TContext>> condition);
        /// <summary>
        /// Returns a builder for creating validation conditions using a switch like syntax.
        /// </summary>
        /// <typeparam name="TValue">Type of the value to switch on</typeparam>
        /// <param name="valueSelector">Delegate that selects the value to switch on</param>
        /// <param name="condition">Optional condition that dictates if the current switch statemant can be executed</param>
        /// <returns>Builder for creating validation conditions using a switch like syntax</returns>
        ISwitchRootConditionConfigurator<TEntity, TError, TContext, TContext, TValue> Switch<TValue>(Func<TEntity, TValue> valueSelector, AsyncPredicate<IValidationRuleContext<TEntity, TContext>> condition = null);

        /// <summary>
        /// Returns a builder for creating validation conditions using a switch like syntax.
        /// </summary>
        /// <typeparam name="TNewContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of the value to switch on</typeparam>
        /// <param name="valueSelector">Delegate that selects the value to switch on</param>
        /// <param name="condition">Optional condition that dictates if the current switch statemant can be executed</param>
        /// <param name="contextRequired">
        /// If a context of <typeparamref name="TNewContext"/> is required for the validation rules created with the returned builder. 
        /// If set to false and context is not of type <typeparamref name="TNewContext"/> then <see cref="IValidationRuleContext{TEntity, TContext}.HasContext"/> will be set to false.
        /// When set to true the rules will only be executed when the supplied context is of type <typeparamref name="TNewContext"/>.
        /// </param>
        /// <returns>Builder for creating validation conditions using a switch like syntax</returns>
        ISwitchRootConditionConfigurator<TEntity, TError, TContext, TNewContext, TValue> Switch<TNewContext, TValue>(Func<TEntity, TValue> valueSelector, Predicate<IValidationRuleContext<TEntity, TNewContext>> condition, bool contextRequired = true) where TNewContext : TContext;
        /// <summary>
        /// Returns a builder for creating validation conditions using a switch like syntax.
        /// </summary>
        /// <typeparam name="TNewContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of the value to switch on</typeparam>
        /// <param name="valueSelector">Delegate that selects the value to switch on</param>
        /// <param name="condition">Optional condition that dictates if the current switch statemant can be executed</param>
        /// <param name="contextRequired">
        /// If a context of <typeparamref name="TNewContext"/> is required for the validation rules created with the returned builder. 
        /// If set to false and context is not of type <typeparamref name="TNewContext"/> then <see cref="IValidationRuleContext{TEntity, TContext}.HasContext"/> will be set to false.
        /// When set to true the rules will only be executed when the supplied context is of type <typeparamref name="TNewContext"/>.
        /// </param>
        /// <returns>Builder for creating validation conditions using a switch like syntax</returns>
        ISwitchRootConditionConfigurator<TEntity, TError, TContext, TNewContext, TValue> Switch<TNewContext, TValue>(Func<TEntity, TValue> valueSelector, AsyncPredicate<IValidationRuleContext<TEntity, TNewContext>> condition = null, bool contextRequired = true) where TNewContext : TContext;
        #endregion
    }
}
