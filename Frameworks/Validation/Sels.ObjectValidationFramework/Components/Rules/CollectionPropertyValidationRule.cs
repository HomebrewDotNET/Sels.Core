using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.ObjectValidationFramework.Validators;
using Sels.ObjectValidationFramework.Contracts.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Sels.Core.Delegates.Async;

namespace Sels.ObjectValidationFramework.Rules
{
    /// <summary>
    /// Validation rule for validating the elements from a property on <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
    /// <typeparam name="TElement">Type of the elements from the property</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal class CollectionPropertyValidationRule<TEntity, TError, TContext, TElement, TValue> : BaseMultiValueValidationRule<TEntity, TError, CollectionPropertyValidationInfo, TContext, TValue>
    {
        // Fields
        private readonly PropertyInfo _property;
        private readonly Func<TElement, TValue> _valueSelector;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="property">The property that is being validated</param>
        /// <param name="valueSelector">Selects the value to validate from an instance of <typeparamref name="TEntity"/></param>
        /// <param name="validator">Validator to delegate <see cref="IValidationConfigurator{TEntity, TError}"/> calls to </param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <param name="globalConditions">Global conditions that all need to pass before any validation rules are allowed to run</param>
        /// <param name="loggers">Option loggers for logging</param>
        public CollectionPropertyValidationRule(PropertyInfo property, Func<TElement, TValue> valueSelector, EntityValidator<TEntity, TError> validator, RuleSettings settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, object>>> globalConditions = null, IEnumerable<ILogger> loggers = null) : base(validator, settings, globalConditions, loggers)
        {
            _valueSelector = valueSelector.ValidateArgument(nameof(valueSelector));
            _property = property.ValidateArgument(nameof(property));
            property.PropertyType.ValidateArgumentAssignableTo(nameof(property), typeof(IEnumerable<TElement>));
        }

        protected override CollectionPropertyValidationInfo CreateInfo(TEntity objectToValidate, object context, int? elementIndex = null, Parent[] parents = null)
        {
            using (_loggers.TraceMethod(this))
            {
                return new CollectionPropertyValidationInfo(_property);
            }
        }

        protected override void ModifyInfo(CollectionPropertyValidationInfo info, TValue value, int index)
        {
            using (_loggers.TraceMethod(this))
            {
                info.ValueIndex = index;
            }
        }

        protected override bool TryGetValueToValidate(TEntity objectToValidate, out IEnumerable<TValue> values)
        {
            using (_loggers.TraceMethod(this))
            {
                values = null;

                if(objectToValidate != null)
                {
                    var elements = _property.GetValue(objectToValidate).CastOrDefault<IEnumerable<TElement>>();

                    if(elements != null)
                    {
                        values = elements.Where(x => x != null).Select(x => _valueSelector(x));
                        return true;
                    }
                }

                return false;
            }
        }
    }

    /// <summary>
    /// Validation rule for validating the elements from a property on <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TElement">Type of the elements from the property</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal class CollectionPropertyValidationRule<TEntity, TError, TElement, TValue> : BaseContextlessValidationRule<TEntity, TError, CollectionPropertyValidationInfo, TValue>
    {
        // Fields
        private readonly PropertyInfo _property;
        private readonly Func<TElement, TValue> _valueSelector;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="property">The property that is being validated</param>
        /// <param name="valueSelector">Selects the value to validate from an instance of <typeparamref name="TEntity"/></param>
        /// <param name="validator">Validator to delegate <see cref="IValidationConfigurator{TEntity, TError}"/> calls to </param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <param name="globalConditions">Global conditions that all need to pass before any validation rules are allowed to run</param>
        /// <param name="loggers">Option loggers for logging</param>
        public CollectionPropertyValidationRule(PropertyInfo property, Func<TElement, TValue> valueSelector, EntityValidator<TEntity, TError> validator, RuleSettings settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, object>>> globalConditions = null, IEnumerable<ILogger> loggers = null) : base(validator, settings, globalConditions, loggers)
        {
            _valueSelector = valueSelector.ValidateArgument(nameof(valueSelector));
            _property = property.ValidateArgument(nameof(property));
            property.PropertyType.ValidateArgumentAssignableTo(nameof(property), typeof(IEnumerable<TElement>));
        }

        protected override BaseContextValidationRule<TEntity, TError, CollectionPropertyValidationInfo, TContext, TValue> CreateNewConfigurator<TContext>()
        {
            using (_loggers.TraceMethod(this))
            {
                return new CollectionPropertyValidationRule<TEntity, TError, TContext, TElement, TValue>(_property, _valueSelector, _validator, _settings, _globalConditions, _loggers);
            }
        }
    }
}
