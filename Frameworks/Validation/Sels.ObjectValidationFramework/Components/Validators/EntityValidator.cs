using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Extensions.Reflection;
using Sels.ObjectValidationFramework.Components.Rules;
using Sels.ObjectValidationFramework.Contracts.Rules;
using Sels.ObjectValidationFramework.Contracts.Validators;
using Sels.ObjectValidationFramework.Templates.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sels.Core.Components.ScopedActions;
using System.Linq.Expressions;

namespace Sels.ObjectValidationFramework.Components.Validators
{
    /// <summary>
    /// Validator that allows for the creation of validation rules for type <typeparamref name="TEntity"/> and using the rules to validate instances of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of object to validate</typeparam>
    /// <typeparam name="TError">Type of validation error that the validator returns</typeparam>
    internal class EntityValidator<TEntity, TError> : EntityValidator<TError>, IValidationConfigurator<TEntity, TError>
    {
        // Fields
        private readonly List<BaseValidationRule<TEntity, TError>> _rules = new List<BaseValidationRule<TEntity, TError>>();
        private readonly List<Predicate<IValidationRuleContext<TEntity, object>>> _currentConditions = new List<Predicate<IValidationRuleContext<TEntity, object>>>();

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="loggers">Loggers for tracing</param>
        internal EntityValidator(IEnumerable<ILogger> loggers = null) : base(loggers)
        {

        }
       
        /// <summary>
        /// Adds a new rule that this validator can use to validate objects of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="rule">Rule to add</param>
        internal void AddNewRule(BaseValidationRule<TEntity, TError> rule)
        {
            using (_loggers.TraceMethod(this))
            {
                rule.ValidateArgument(nameof(rule));

                _rules.Add(rule);
            }
        }

        #region Configuration
        public IValidationRuleConfigurator<TEntity, TError, CollectionPropertyValidationInfo, TElement> ForElements<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> property)
        {
            using (_loggers.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));

                return ForElements(property, x => x);
            }
        }
        /// <inheritdoc/>
        public IValidationRuleConfigurator<TEntity, TError, CollectionPropertyValidationInfo, TValue> ForElements<TElement, TValue>(Expression<Func<TEntity, IEnumerable<TElement>>> property, Func<TElement, TValue> valueSelector)
        {
            using (_loggers.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                valueSelector.ValidateArgument(nameof(valueSelector));              
                if(!property.TryExtractProperty(out var propertyInfo) || !propertyInfo.ReflectedType.IsAssignableTo<TEntity>())
                {
                    throw new ArgumentException($"{nameof(property)} must select a property on {typeof(TEntity)}");
                }

                return new CollectionPropertyValidationRule<TEntity, TError, TElement, TValue>(propertyInfo, valueSelector, this, _currentConditions, _loggers);
            }
        }
        /// <inheritdoc/>
        public IValidationRuleConfigurator<TEntity, TError, PropertyValidationInfo, TPropertyValue> ForProperty<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> property)
        {
            using (_loggers.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                if (!property.TryExtractProperty(out var propertyInfo) || !typeof(TEntity).IsAssignableTo(propertyInfo.ReflectedType))
                {
                    throw new ArgumentException($"{nameof(property)} must select a property on {typeof(TEntity)}");
                }

                return new PropertyValidationRule<TEntity, TError, TPropertyValue, TPropertyValue>(propertyInfo, false, x => x, this, _currentConditions, _loggers);
            }
        }
        /// <inheritdoc/>
        public IValidationRuleConfigurator<TEntity, TError, PropertyValidationInfo, TValue> ForProperty<TPropertyValue, TValue>(Expression<Func<TEntity, TPropertyValue>> property, Func<TPropertyValue, TValue> valueSelector)
        {
            using (_loggers.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                valueSelector.ValidateArgument(nameof(valueSelector));
                if (!property.TryExtractProperty(out var propertyInfo) || !propertyInfo.ReflectedType.IsAssignableTo<TEntity>())
                {
                    throw new ArgumentException($"{nameof(property)} must select a property on {typeof(TEntity)}");
                }

                return new PropertyValidationRule<TEntity, TError, TPropertyValue, TValue>(propertyInfo, true, valueSelector, this, _currentConditions, _loggers);
            }
        }
        /// <inheritdoc/>
        public IValidationRuleConfigurator<TEntity, TError, NullValidationInfo, TEntity> ForSource()
        {
            using (_loggers.TraceMethod(this))
            {
                return ForSource(x => x);
            }
        }
        /// <inheritdoc/>
        public IValidationRuleConfigurator<TEntity, TError, NullValidationInfo, TValue> ForSource<TValue>(Func<TEntity, TValue> valueSelector)
        {
            using (_loggers.TraceMethod(this))
            {
                valueSelector.ValidateArgument(nameof(valueSelector));
                
                return new SourceValidationRule<TEntity, TError, TValue>(valueSelector, this, _currentConditions, _loggers);
            }
        }
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TError> ValidateWhen(Predicate<IValidationRuleContext<TEntity, object>> condition, Action<IValidationConfigurator<TEntity, TError>> configurator)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                configurator.ValidateArgument(nameof(configurator));

                using (new ScopedAction(() => _currentConditions.Add(condition), () => _currentConditions.Remove(condition)))
                {
                    configurator(this);
                }

                return this;
            }
        }
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TError> ValidateWhen<TContext>(Predicate<IValidationRuleContext<TEntity, TContext>> condition, Action<IValidationConfigurator<TEntity, TError>> configurator)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                configurator.ValidateArgument(nameof(configurator));

                return ValidateWhen(x => {
                    var context = new ValidationRuleContext<TEntity, TContext>(x.Source, x.Context, x.ElementIndex, x.Parents);
                    return condition(context);
                }, configurator);
            }
        }
        #endregion

        #region Validation
        /// <inheritdoc/>
        internal override bool CanValidate(object objectToValidate, object context, int? elementIndex = null, Parent[] parents = null)
        {
            using (_loggers.TraceMethod(this))
            {
                return objectToValidate != null && objectToValidate.IsAssignableTo<TEntity>();
            }
        }

        /// <inheritdoc/>
        internal override TError[] Validate(object objectToValidate, object context, int? elementIndex = null, Parent[] parents = null)
        {
            using (_loggers.TraceMethod(this))
            {
                objectToValidate.ValidateArgument(x => CanValidate(objectToValidate, context, elementIndex, parents), x => new NotSupportedException($"Current validator cannot validate <{(objectToValidate == null ? "null references" : objectToValidate.GetType().ToString())}>"));

                var typedObjectToValidate = objectToValidate.As<TEntity>();
                var validationRuleContext = new ValidationRuleContext<TEntity, object>(typedObjectToValidate, context, elementIndex, parents);

                return _rules.Where(rule => rule.CanValidate(validationRuleContext)).SelectMany(rule => rule.Validate(typedObjectToValidate, context, elementIndex, parents) ?? new TError[0]).ToArray();
            }
        }
        #endregion
    }

    /// <summary>
    /// Untyped validator that exposes methods for validating objects.
    /// </summary>
    /// <typeparam name="TError">Type of validation error that the validator returns</typeparam>
    internal abstract class EntityValidator<TError>
    {
        // Fields
        protected readonly IEnumerable<ILogger> _loggers;

        public EntityValidator(IEnumerable<ILogger> loggers = null)
        {
            _loggers = loggers;
        }

        #region Validation
        /// <summary>
        /// Checks if this validator can validate <paramref name="objectToValidate"/>.
        /// </summary>
        /// <param name="objectToValidate">Object to validate</param>
        /// <param name="context">Optional context that can be supplied to a validation rule.</param>
        /// <param name="elementIndex">Index of <paramref name="objectToValidate"/> if it was part of a collection. Will be null if it wasn't part of a collection.</param>
        /// <param name="parents">Hierarchy of object if property fallthrough is enabled. The previous element is always the parent of the next element.</param>
        /// <returns>If this validator can validate <paramref name="objectToValidate"/></returns>
        internal abstract bool CanValidate(object objectToValidate, object context, int? elementIndex = null, Parent[] parents = null);
        /// <summary>
        /// Validates <paramref name="objectToValidate"/> and returns the validation errors if they occured for <paramref name="objectToValidate"/>.
        /// </summary>
        /// <param name="objectToValidate">Object to validate</param>
        /// <param name="context">Optional context that can be supplied to a validation rule.</param>
        /// <param name="elementIndex">Index of <paramref name="objectToValidate"/> if it was part of a collection. Will be null if it wasn't part of a collection.</param>
        /// <param name="parents">Hierarchy of object if property fallthrough is enabled. The previous element is always the parent of the next element.</param>
        /// <returns>All the validation errors for <paramref name="objectToValidate"/></returns>
        internal abstract TError[] Validate(object objectToValidate, object context, int? elementIndex = null, Parent[] parents = null);
        #endregion
    }
}
