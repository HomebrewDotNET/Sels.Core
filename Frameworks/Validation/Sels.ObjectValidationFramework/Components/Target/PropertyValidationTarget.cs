using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.ObjectValidationFramework.Validators;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Sels.Core.Delegates.Async;
using Sels.ObjectValidationFramework.Rules;
using Sels.Core.Extensions.Reflection;
using System.Xml.Linq;
using Sels.ObjectValidationFramework.Profile;

namespace Sels.ObjectValidationFramework.Target
{
    /// <summary>
    /// Validation rule for validating a property on <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
    /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
    /// <typeparam name="TPropertyValue">Type of the property</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal class PropertyValidationTarget<TEntity, TError, TBaseContext, TTargetContext, TPropertyValue, TValue> : BaseSingleValueValidationTarget<TEntity, TError, TBaseContext, PropertyValidationInfo, TTargetContext, TValue> where TTargetContext : TBaseContext
    {
        // Fields
        private readonly PropertyInfo _property;
        private readonly bool _isSubSelection;
        private readonly Func<TEntity, TPropertyValue> _propertyGetter;
        private readonly Func<TPropertyValue, TValue> _valueSelector;

        // Properties
        public override string Identifier => $"Property.{typeof(TEntity).GetDisplayName(false)}.{_property.Name}";

        /// <inheritdoc cref="PropertyValidationTarget{TEntity, TError, TBaseContext, TTargetContext, TPropertyValue, TValue}"/>
        /// <param name="property">The property that is being validated</param>
        /// <param name="isSubSelection">If <paramref name="valueSelector"/> selects a value from property. False if it just returns the property value</param>
        ///  <param name="propertyGetter">The delegate that selects the value from the property to validate</param>
        /// <param name="valueSelector">Selects the value to validate from an instance of <typeparamref name="TEntity"/></param>
        /// <param name="validator"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._validator"/></param>
        /// <param name="settings"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._settings"/></param>
        /// <param name="globalConditions"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._globalConditions"/></param>
        /// <param name="logger"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._logger"/></param>
        public PropertyValidationTarget(PropertyInfo property, bool isSubSelection, Func<TEntity, TPropertyValue> propertyGetter, Func<TPropertyValue, TValue> valueSelector, EntityValidator<TEntity, TBaseContext, TError> validator, TargetExecutionOptions settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, TBaseContext>>> globalConditions = null, ILogger logger = null) : base(validator, settings, globalConditions, logger)
        {
            _valueSelector = valueSelector.ValidateArgument(nameof(valueSelector));
            _property = property.ValidateArgument(nameof(property));
            _propertyGetter = propertyGetter.ValidateArgument(nameof(propertyGetter));
            property.PropertyType.ValidateArgumentAssignableTo(nameof(property), typeof(TPropertyValue));
            _isSubSelection = isSubSelection;
        }

        /// <inheritdoc/>
        protected override PropertyValidationInfo CreateInfo(TEntity objectToValidate, TTargetContext context, int? elementIndex = null, Parent[] parents = null)
        {
            using (_logger.TraceMethod(this))
            {
                return new PropertyValidationInfo(_property);
            }
        }

        /// <inheritdoc/>
        protected override bool TryGetValueToValidate(TEntity objectToValidate, out TValue value)
        {
            using (_logger.TraceMethod(this))
            {
                value = default;
                var propertyValue = _propertyGetter(objectToValidate);

                if (propertyValue != null)
                {
                    value = _valueSelector(propertyValue);
                    return true;
                }
                // Nulls are allowed when there is no subselection
                else if (!_isSubSelection)
                {
                    value = propertyValue.CastToOrDefault<TValue>();
                    return true;
                }

                _logger.Debug($"{Identifier}: Property is null so can't select sub value so can't validate");

                return false;
            }
        }

        /// <inheritdoc/>
        protected override void ModifyError(ValidationError<TError> error)
        {
            error.Property = _property;
        }
        /// <inheritdoc/>
        protected override string GetDisplayNameFor(ValidationRuleContext<TEntity, PropertyValidationInfo, TTargetContext, TValue> context, bool includeParents) => context.GetFullDisplayName(includeParents);
    }

    /// <summary>
    /// Validation rule for validating a property on <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TContext">Type of the validation context used by the current validator</typeparam>
    /// <typeparam name="TPropertyValue">Type of the property</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal class PropertyValidationTarget<TEntity, TError, TContext, TPropertyValue, TValue> : BaseContextlessValidationTarget<TEntity, TError, TContext, PropertyValidationInfo, TValue>
    {
        // Fields
        private readonly PropertyInfo _property;
        private readonly bool _isSubSelection;
        private readonly Func<TEntity, TPropertyValue> _propertyGetter;
        private readonly Func<TPropertyValue, TValue> _valueSelector;

        // Properties
        public override string Identifier => $"Property.{typeof(TEntity).GetDisplayName(false)}.{_property.Name}";

        /// <inheritdoc cref="PropertyValidationTarget{TEntity, TError, TContext, TPropertyValue, TValue}"/>
        /// <param name="property">The property that is being validated</param>
        /// <param name="isSubSelection">If <paramref name="valueSelector"/> selects a value from property. False if it just returns the property value</param>
        ///  <param name="propertyGetter">The delegate that selects the value from the property to validate</param>
        /// <param name="valueSelector">Selects the value to validate from an instance of <typeparamref name="TEntity"/></param>
        /// <param name="validator"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._validator"/></param>
        /// <param name="settings"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._settings"/></param>
        /// <param name="globalConditions"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._globalConditions"/></param>
        /// <param name="logger"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._logger"/></param>
        public PropertyValidationTarget(PropertyInfo property, bool isSubSelection, Func<TEntity, TPropertyValue> propertyGetter, Func<TPropertyValue, TValue> valueSelector, EntityValidator<TEntity, TContext, TError> validator, TargetExecutionOptions settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, TContext>>> globalConditions = null, ILogger logger = null) : base(validator, settings, globalConditions, logger)
        {
            _valueSelector = valueSelector.ValidateArgument(nameof(valueSelector));
            _property = property.ValidateArgument(nameof(property));
            _propertyGetter = propertyGetter.ValidateArgument(nameof(propertyGetter));
            property.PropertyType.ValidateArgumentAssignableTo(nameof(property), typeof(TPropertyValue));
            _isSubSelection = isSubSelection;
        }

        /// <inheritdoc/>
        protected override BaseContextValidationTarget<TEntity, TError, TContext, PropertyValidationInfo, TNewContext, TValue> CreateNewConfigurator<TNewContext>()
        {
            using (_logger.TraceMethod(this))
            {
                return new PropertyValidationTarget<TEntity, TError, TContext, TNewContext, TPropertyValue, TValue>(_property, _isSubSelection, _propertyGetter, _valueSelector, _validator, _settings, _globalConditions, _logger);
            }
        }
    }
}
