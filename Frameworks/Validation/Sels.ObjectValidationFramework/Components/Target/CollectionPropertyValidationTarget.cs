using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.ObjectValidationFramework.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Sels.Core.Delegates.Async;
using Sels.Core.Extensions.Reflection;
using Sels.ObjectValidationFramework.Rules;
using Sels.ObjectValidationFramework.Profile;

namespace Sels.ObjectValidationFramework.Target
{
    /// <summary>
    /// Validation target for validating the elements from a property on <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
    /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    /// <typeparam name="TElement">The type of the elements to validate</typeparam>
    internal class CollectionPropertyValidationTarget<TEntity, TError, TBaseContext, TTargetContext, TElement, TValue> : BaseMultiValueValidationTarget<TEntity, TError, TBaseContext, CollectionPropertyValidationInfo, TTargetContext, TValue> where TTargetContext : TBaseContext
    {
        // Fields
        private readonly PropertyInfo _property;
        private readonly Func<TEntity, IEnumerable<TElement>> _propertyGetter;
        private readonly Func<TElement, TValue> _valueSelector;

        // Properties
        public override string Identifier => $"CollectionProperty.{typeof(TEntity).GetDisplayName(false)}.{_property.Name}";

        /// <inheritdoc cref="CollectionPropertyValidationTarget{TEntity, TError, TBaseContext, TTargetContext, TElement, TValue}"/>
        /// <param name="property">The property that is being validated</param>
        /// <param name="propertyGetter">The delegate that selects the value from the property to validate</param>
        /// <param name="valueSelector">Selects the value to validate from an instance of <typeparamref name="TEntity"/></param>
        /// <param name="validator"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._validator"/></param>
        /// <param name="settings"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._settings"/></param>
        /// <param name="globalConditions"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._globalConditions"/></param>
        /// <param name="logger"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._logger"/></param>
        public CollectionPropertyValidationTarget(PropertyInfo property, Func<TEntity, IEnumerable<TElement>> propertyGetter, Func<TElement, TValue> valueSelector, EntityValidator<TEntity, TBaseContext, TError> validator, TargetExecutionOptions settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, TBaseContext>>> globalConditions = null, ILogger logger = null) : base(validator, settings, globalConditions, logger)
        {
            _valueSelector = valueSelector.ValidateArgument(nameof(valueSelector));
            _property = property.ValidateArgument(nameof(property));
            _propertyGetter = propertyGetter.ValidateArgument(nameof(propertyGetter));
            property.PropertyType.ValidateArgumentAssignableTo(nameof(property), typeof(IEnumerable<TElement>));
        }

        /// <inheritdoc/>
        protected override CollectionPropertyValidationInfo CreateInfo(TEntity objectToValidate, TTargetContext context, int? elementIndex = null, Parent[] parents = null)
        {
            using (_logger.TraceMethod(this))
            {
                return new CollectionPropertyValidationInfo(_property);
            }
        }
        /// <inheritdoc/>
        protected override void ModifyInfo(CollectionPropertyValidationInfo info, TValue value, int index)
        {
            using (_logger.TraceMethod(this))
            {
                info.ValueIndex = index;
            }
        }
        /// <inheritdoc/>
        protected override bool TryGetValueToValidate(TEntity objectToValidate, out IEnumerable<TValue> values)
        {
            using (_logger.TraceMethod(this))
            {
                values = null;

                var elements = _propertyGetter(objectToValidate);

                if (elements == null)
                {
                    _logger.Debug($"{Identifier}: Source collection is null so can't validate");
                    return false;
                }

                values = elements.Select(x => x != null ? _valueSelector(x) : default);
                return true;
            }
        }
        /// <inheritdoc/>
        protected override void ModifyError(ValidationError<TError> error)
        {
            error.Property = _property;
        }
        /// <inheritdoc/>
        protected override string GetDisplayNameFor(ValidationRuleContext<TEntity, CollectionPropertyValidationInfo, TTargetContext, TValue> context, bool includeParents) => context.GetFullDisplayName(includeParents);
    }

    /// <summary>
    /// Validation rule for validating the elements from a property on <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TContext">Type of the validation context used by the current validator</typeparam>
    /// <typeparam name="TElement">Type of the elements from the property</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal class CollectionPropertyValidationTarget<TEntity, TError, TContext, TElement, TValue> : BaseContextlessValidationTarget<TEntity, TError, TContext, CollectionPropertyValidationInfo, TValue>
    {
        // Fields
        private readonly PropertyInfo _property;
        private readonly Func<TEntity, IEnumerable<TElement>> _propertyGetter;
        private readonly Func<TElement, TValue> _valueSelector;

        // Properties
        public override string Identifier => $"CollectionProperty.{typeof(TEntity).GetDisplayName(false)}.{_property.Name}";

        /// <inheritdoc cref="CollectionPropertyValidationTarget{TEntity, TError, TContext, TElement, TValue}"/>
        /// <param name="property">The property that is being validated</param>
        /// <param name="propertyGetter">The delegate that selects the value from the property to validate</param>
        /// <param name="valueSelector">Selects the value to validate from an instance of <typeparamref name="TEntity"/></param>
        /// <param name="validator"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._validator"/></param>
        /// <param name="settings"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._settings"/></param>
        /// <param name="globalConditions"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._globalConditions"/></param>
        /// <param name="logger"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._logger"/></param>
        public CollectionPropertyValidationTarget(PropertyInfo property, Func<TEntity, IEnumerable<TElement>> propertyGetter, Func<TElement, TValue> valueSelector, EntityValidator<TEntity, TContext, TError> validator, TargetExecutionOptions settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, TContext>>> globalConditions = null, ILogger logger = null) : base(validator, settings, globalConditions, logger)
        {
            _valueSelector = valueSelector.ValidateArgument(nameof(valueSelector));
            _property = property.ValidateArgument(nameof(property));
            _propertyGetter = propertyGetter.ValidateArgument(nameof(propertyGetter));
            property.PropertyType.ValidateArgumentAssignableTo(nameof(property), typeof(IEnumerable<TElement>));
        }

        /// <inheritdoc/>
        protected override BaseContextValidationTarget<TEntity, TError, TContext, CollectionPropertyValidationInfo, TNewContext, TValue> CreateNewConfigurator<TNewContext>()
        {
            using (_logger.TraceMethod(this))
            {
                return new CollectionPropertyValidationTarget<TEntity, TError, TContext, TNewContext, TElement, TValue>(_property, _propertyGetter, _valueSelector, _validator, _settings, _globalConditions, _logger);
            }
        }
    }
}
