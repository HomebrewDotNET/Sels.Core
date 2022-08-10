using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.ObjectValidationFramework.Components.Validators;
using Sels.ObjectValidationFramework.Contracts.Rules;
using Sels.ObjectValidationFramework.Contracts.Validators;
using Sels.ObjectValidationFramework.Models;
using Sels.ObjectValidationFramework.Templates.Rules;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static Sels.Core.Delegates.Async;

namespace Sels.ObjectValidationFramework.Components.Rules
{
    /// <summary>
    /// Validation rule for validating a property on <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
    /// <typeparam name="TPropertyValue">Type of the property</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal class PropertyValidationRule<TEntity, TError, TContext, TPropertyValue, TValue> : BaseSingleValueValidationRule<TEntity, TError, PropertyValidationInfo, TContext, TValue>
    {
        // Fields
        private readonly PropertyInfo _property;
        private readonly bool _isSubSelection;
        private readonly Func<TPropertyValue, TValue> _valueSelector;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="property">The property that is being validated</param>
        /// <param name="isSubSelection">If <paramref name="valueSelector"/> selects a value from property. False if it just returns the property value</param>
        /// <param name="valueSelector">Selects the value to validate from an instance of <typeparamref name="TEntity"/></param>
        /// <param name="validator">Validator to delegate <see cref="IValidationConfigurator{TEntity, TError}"/> calls to </param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <param name="globalConditions">Global conditions that all need to pass before any validation rules are allowed to run</param>
        /// <param name="loggers">Option loggers for logging</param>
        public PropertyValidationRule(PropertyInfo property, bool isSubSelection, Func<TPropertyValue, TValue> valueSelector, EntityValidator<TEntity, TError> validator, RuleSettings settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, object>>> globalConditions = null, IEnumerable<ILogger> loggers = null) : base(validator, settings, globalConditions, loggers)
        {
            _valueSelector = valueSelector.ValidateArgument(nameof(valueSelector));
            _property = property.ValidateArgument(nameof(property));
            property.PropertyType.ValidateArgumentAssignableTo(nameof(property), typeof(TPropertyValue));
            _isSubSelection = isSubSelection;
        }

        /// <inheritdoc/>
        protected override PropertyValidationInfo CreateInfo(TEntity objectToValidate, object context, int? elementIndex = null, Parent[] parents = null)
        {
            using (_loggers.TraceMethod(this))
            {
                return new PropertyValidationInfo(_property);
            }
        }

        /// <inheritdoc/>
        protected override bool TryGetValueToValidate(TEntity objectToValidate, out TValue value)
        {
            using (_loggers.TraceMethod(this))
            {
                value = default;
                if(objectToValidate != null)
                {
                    var propertyValue = _property.GetValue(objectToValidate).CastOrDefault<TPropertyValue>();

                    if(propertyValue != null)
                    {
                        value = _valueSelector(propertyValue);
                        return true;
                    }
                    // Nulls are allowed when there is no subselection
                    else if (!_isSubSelection){
                        value = propertyValue.CastOrDefault<TValue>();
                        return true;
                    }
                    
                }
                
                return false;
            }
        }
    }

    /// <summary>
    /// Validation rule for validating a property on <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TPropertyValue">Type of the property</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal class PropertyValidationRule<TEntity, TError, TPropertyValue, TValue> : BaseContextlessValidationRule<TEntity, TError, PropertyValidationInfo, TValue>
    {
        // Fields
        private readonly PropertyInfo _property;
        private readonly bool _isSubSelection;
        private readonly Func<TPropertyValue, TValue> _valueSelector;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="property">The property that is being validated</param>
        /// <param name="isSubSelection">If <paramref name="valueSelector"/> selects a value from property. False if it just returns the property value</param>
        /// <param name="valueSelector">Selects the value to validate from an instance of <typeparamref name="TEntity"/></param>
        /// <param name="validator">Validator to delegate <see cref="IValidationConfigurator{TEntity, TError}"/> calls to </param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <param name="globalConditions">Global conditions that all need to pass before any validation rules are allowed to run</param>
        /// <param name="loggers">Option loggers for logging</param>
        public PropertyValidationRule(PropertyInfo property, bool isSubSelection, Func<TPropertyValue, TValue> valueSelector, EntityValidator<TEntity, TError> validator, RuleSettings settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, object>>> globalConditions = null, IEnumerable<ILogger> loggers = null) : base(validator, settings, globalConditions, loggers)
        {
            _valueSelector = valueSelector.ValidateArgument(nameof(valueSelector));
            _property = property.ValidateArgument(nameof(property));
            property.PropertyType.ValidateArgumentAssignableTo(nameof(property), typeof(TPropertyValue));
            _isSubSelection = isSubSelection;
        }

        /// <inheritdoc/>
        protected override BaseContextValidationRule<TEntity, TError, PropertyValidationInfo, TContext, TValue> CreateNewConfigurator<TContext>()
        {
            using (_loggers.TraceMethod(this))
            {
                return new PropertyValidationRule<TEntity, TError, TContext, TPropertyValue, TValue>(_property, _isSubSelection, _valueSelector, _validator, _settings, _globalConditions, _loggers);
            }
        }
    }
}
