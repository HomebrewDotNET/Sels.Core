using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Sels.ObjectValidationFramework.Components.Rules;
using Sels.ObjectValidationFramework.Contracts.Rules;
using Sels.ObjectValidationFramework.Models;

namespace Sels.ObjectValidationFramework.Contracts.Validators
{
    /// <summary>
    /// Configurator for creating validation rules for objects of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of object to create validation rules for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    public interface IValidationConfigurator<TEntity, TError>
    {
        /// <summary>
        /// Creates a configurator for creating validation rules for <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="settings">Extra settings for the rule</param>
        /// <returns>Configurator for creating validation rules</returns>
        IValidationRuleConfigurator<TEntity, TError, NullValidationInfo, TEntity> ForSource(RuleSettings settings = RuleSettings.None);

        /// <summary>
        /// Creates a configurator for creating validation rules for <typeparamref name="TValue"/> selected from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="valueSelector">Selects the value to validate from an instance of <typeparamref name="TEntity"/></param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <typeparam name="TValue">Type of value to validate</typeparam>
        /// <returns>Configurator for creating validation rules</returns>
        IValidationRuleConfigurator<TEntity, TError, NullValidationInfo, TValue> ForSource<TValue>(Func<TEntity, TValue> valueSelector, RuleSettings settings = RuleSettings.None);

        /// <summary>
        /// Creates a configurator for creating validation rules for <paramref name="property"/> on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">Delegate that selects which property to validate. Will throw an <see cref="ArgumentException"/> when anything but a property is selected</param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <typeparam name="TPropertyValue">Type of property to validate</typeparam>
        /// <returns>Configurator for creating validation rules</returns>
        IValidationRuleConfigurator<TEntity, TError, PropertyValidationInfo, TPropertyValue> ForProperty<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> property, RuleSettings settings = RuleSettings.None);

        /// <summary>
        /// Creates a configurator for creating validation rules for the value selected by <paramref name="valueSelector"/> from <paramref name="property"/> on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">Delegate that selects which property to validate. Will throw an <see cref="ArgumentException"/> when anything but a property is selected</param>
        /// <param name="valueSelector">Delegate that selects the value to validate on <paramref name="property"/></param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <typeparam name="TPropertyValue">Type of property to validate</typeparam>
        /// <typeparam name="TValue">Type of value to validate</typeparam>
        /// <returns>Configurator for creating validation rules</returns>
        IValidationRuleConfigurator<TEntity, TError, PropertyValidationInfo, TValue> ForProperty<TPropertyValue, TValue>(Expression<Func<TEntity, TPropertyValue>> property, Func<TPropertyValue, TValue> valueSelector, RuleSettings settings = RuleSettings.None);

        /// <summary>
        /// Creates a configurator for creating validation rules for the elements in <paramref name="property"/> on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">Delegate that selects which property to validate. Will throw an <see cref="ArgumentException"/> when anything but a property is selected</param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <typeparam name="TElement">Type of element to validate</typeparam>
        /// <returns>Configurator for creating validation rules</returns>
        IValidationRuleConfigurator<TEntity, TError, CollectionPropertyValidationInfo, TElement> ForElements<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> property, RuleSettings settings = RuleSettings.None);

        /// <summary>
        /// Creates a configurator for creating validation rules for the value selected by <paramref name="valueSelector"/> for each element in <paramref name="property"/> on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">Delegate that selects which property to validate. Will throw an <see cref="ArgumentException"/> when anything but a property is selected</param>
        /// <param name="valueSelector">Delegate that selects the value to validate on each element in <paramref name="property"/></param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <typeparam name="TElement">Type of element to validate</typeparam>
        /// <typeparam name="TValue">Type of value to validate</typeparam>
        /// <returns>Configurator for creating validation rules</returns>
        IValidationRuleConfigurator<TEntity, TError, CollectionPropertyValidationInfo, TValue> ForElements<TElement, TValue>(Expression<Func<TEntity, IEnumerable<TElement>>> property, Func<TElement, TValue> valueSelector, RuleSettings settings = RuleSettings.None);

        /// <summary>
        /// All validation rules created with <paramref name="configurator"/> will only be executed when <paramref name="condition"/> passes.
        /// </summary>
        /// <param name="condition">Delegate that checks if validation rules created with <paramref name="configurator"/> can be executed</param>
        /// <param name="configurator">Configurator for creating validation rules</param>
        /// <returns>Configurator for creating validation rules</returns>
        IValidationConfigurator<TEntity, TError> ValidateWhen(Predicate<IValidationRuleContext<TEntity, object>> condition, Action<IValidationConfigurator<TEntity, TError>> configurator);

        /// <summary>
        /// All validation rules created with <paramref name="configurator"/> will only be executed when <paramref name="condition"/> passes. Condition uses a context of type <typeparamref name="TContext"/>.
        /// </summary>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="condition">Delegate that checks if validation rules created with <paramref name="configurator"/> can be executed</param>
        /// <param name="configurator">Configurator for creating validation rules</param>
        /// <returns>Configurator for creating validation rules</returns>
        IValidationConfigurator<TEntity, TError> ValidateWhen<TContext>(Predicate<IValidationRuleContext<TEntity, TContext>> condition, Action<IValidationConfigurator<TEntity, TError>> configurator);
        /// <summary>
        /// All validation rules created fter calling this method will only be executed when <paramref name="condition"/> passes.
        /// </summary>
        /// <param name="condition">Delegate that checks if validation rules can be executed</param>
        /// <returns>Configurator for creating validation rules</returns>
        IValidationConfigurator<TEntity, TError> ValidateWhen(Predicate<IValidationRuleContext<TEntity, object>> condition);
        /// <summary>
        /// All validation rules created fter calling this method will only be executed when <paramref name="condition"/> passes. Condition uses a context of type <typeparamref name="TContext"/>.
        /// </summary>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="condition">Delegate that checks if validation rules can be executed</param>
        /// <returns>Configurator for creating validation rules</returns>
        IValidationConfigurator<TEntity, TError> ValidateWhen<TContext>(Predicate<IValidationRuleContext<TEntity, TContext>> condition);
    }
}
