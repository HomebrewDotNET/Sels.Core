using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Extensions.Reflection;
using Sels.ObjectValidationFramework.Rules;
using Sels.ObjectValidationFramework.Contracts.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using Sels.Core.Components.Scope;
using System.Linq.Expressions;
using Sels.Core.Extensions.Logging;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.ObjectValidationFramework.Validators
{
    /// <inheritdoc cref="IValidationConfigurator{TEntity, TContext, TError}"/>
    internal class EntityValidator<TEntity, TContext, TError> : IValidationConfigurator<TEntity, TContext, TError>
    {
        // Fields
        private readonly bool _contextRequired;
        private readonly IValidationConfigurator<TEntity, TError> _parent;

        /// <inheritdoc cref="EntityValidator{TEntity, TContext, TError}"/>
        /// <param name="contextRequired">If the context is required by the created rules</param>
        /// <param name="parent">The parent validator to delegate calls to</param>
        public EntityValidator(bool contextRequired, IValidationConfigurator<TEntity, TError> parent)
        {
            _contextRequired = contextRequired;
            _parent = parent.ValidateArgument(nameof(parent));
        }

        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TContext, TError> ValidateWhen(Predicate<IValidationRuleContext<TEntity, TContext>> condition, Action<IValidationConfigurator<TEntity, TContext, TError>> configurator)
        {
            _parent.ValidateWhen(condition, configurator);
            return this;
        }
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TContext, TError> ValidateWhen<TNewContext>(Predicate<IValidationRuleContext<TEntity, TNewContext>> condition, Action<IValidationConfigurator<TEntity, TNewContext, TError>> configurator) where TNewContext : TContext
        {
            _parent.ValidateWhen(condition, configurator);
            return this;
        }
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TContext, TError> ValidateWhen(Predicate<IValidationRuleContext<TEntity, TContext>> condition)
        {
            _parent.ValidateWhen(condition);
            return this;
        }
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TContext, TError> ValidateWhen<TNewContext>(Predicate<IValidationRuleContext<TEntity, TNewContext>> condition) where TNewContext : TContext
        {
            _parent.ValidateWhen(condition);
            return this;
        }

        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TContext, TError> ValidateWhen(AsyncPredicate<IValidationRuleContext<TEntity, TContext>> condition, Action<IValidationConfigurator<TEntity, TContext, TError>> configurator)
        {
            _parent.ValidateWhen(condition, configurator);
            return this;
        }
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TContext, TError> ValidateWhen<TNewContext>(AsyncPredicate<IValidationRuleContext<TEntity, TNewContext>> condition, Action<IValidationConfigurator<TEntity, TNewContext, TError>> configurator) where TNewContext : TContext
        {
            _parent.ValidateWhen(condition, configurator);
            return this;
        }
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TContext, TError> ValidateWhen(AsyncPredicate<IValidationRuleContext<TEntity, TContext>> condition)
        {
            _parent.ValidateWhen(condition);
            return this;
        }
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TContext, TError> ValidateWhen<TNewContext>(AsyncPredicate<IValidationRuleContext<TEntity, TNewContext>> condition) where TNewContext : TContext
        {
            _parent.ValidateWhen(condition);
            return this;
        }

        /// <inheritdoc/>
        IValidationRuleConfigurator<TEntity, TError, CollectionPropertyValidationInfo, TContext, TElement> IValidationConfigurator<TEntity, TContext, TError>.ForElements<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> property, RuleSettings settings)
        {
            return _parent.ForElements(property, settings).WithContext<TContext>(_contextRequired);
        }
        /// <inheritdoc/>
        IValidationRuleConfigurator<TEntity, TError, CollectionPropertyValidationInfo, TContext, TValue> IValidationConfigurator<TEntity, TContext, TError>.ForElements<TElement, TValue>(Expression<Func<TEntity, IEnumerable<TElement>>> property, Func<TElement, TValue> valueSelector, RuleSettings settings)
        {
            return _parent.ForElements(property, valueSelector, settings).WithContext<TContext>(_contextRequired);
        }
        /// <inheritdoc/>
        IValidationRuleConfigurator<TEntity, TError, PropertyValidationInfo, TContext, TPropertyValue> IValidationConfigurator<TEntity, TContext, TError>.ForProperty<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> property, RuleSettings settings)
        {
            return _parent.ForProperty(property, settings).WithContext<TContext>(_contextRequired);
        }
        /// <inheritdoc/>
        IValidationRuleConfigurator<TEntity, TError, PropertyValidationInfo, TContext, TValue> IValidationConfigurator<TEntity, TContext, TError>.ForProperty<TPropertyValue, TValue>(Expression<Func<TEntity, TPropertyValue>> property, Func<TPropertyValue, TValue> valueSelector, RuleSettings settings)
        {
            return _parent.ForProperty(property, valueSelector, settings).WithContext<TContext>(_contextRequired);
        }
        /// <inheritdoc/>
        IValidationRuleConfigurator<TEntity, TError, NullValidationInfo, TContext, TEntity> IValidationConfigurator<TEntity, TContext, TError>.ForSource(RuleSettings settings)
        {
            return _parent.ForSource(settings).WithContext<TContext>(_contextRequired);
        }
        /// <inheritdoc/>
        IValidationRuleConfigurator<TEntity, TError, NullValidationInfo, TContext, TValue> IValidationConfigurator<TEntity, TContext, TError>.ForSource<TValue>(Func<TEntity, TValue> valueSelector, RuleSettings settings)
        {
            return _parent.ForSource(valueSelector, settings).WithContext<TContext>(_contextRequired);
        }
    }

    /// <summary>
    /// Validator that allows for the creation of validation rules for type <typeparamref name="TEntity"/> and using the rules to validate instances of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of object to validate</typeparam>
    /// <typeparam name="TError">Type of validation error that the validator returns</typeparam>
    internal class EntityValidator<TEntity, TError> : EntityValidator<TError>, IValidationConfigurator<TEntity, TError>
    {
        // Fields
        private readonly List<BaseValidationRule<TEntity, TError>> _rules = new List<BaseValidationRule<TEntity, TError>>();
        private readonly List<AsyncPredicate<IValidationRuleContext<TEntity, object>>> _currentConditions = new List<AsyncPredicate<IValidationRuleContext<TEntity, object>>>();

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
        /// <inheritdoc/>
        public IValidationRuleConfigurator<TEntity, TError, CollectionPropertyValidationInfo, TElement> ForElements<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> property, RuleSettings settings = RuleSettings.None)
        {
            using (_loggers.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));

                return ForElements(property, x => x, settings);
            }
        }
        /// <inheritdoc/>
        public IValidationRuleConfigurator<TEntity, TError, CollectionPropertyValidationInfo, TValue> ForElements<TElement, TValue>(Expression<Func<TEntity, IEnumerable<TElement>>> property, Func<TElement, TValue> valueSelector, RuleSettings settings = RuleSettings.None)
        {
            using (_loggers.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                valueSelector.ValidateArgument(nameof(valueSelector));              
                if(!property.TryExtractProperty(out var propertyInfo) || !propertyInfo.ReflectedType.IsAssignableTo<TEntity>())
                {
                    throw new ArgumentException($"{nameof(property)} must select a property on {typeof(TEntity)}");
                }

                return new CollectionPropertyValidationRule<TEntity, TError, TElement, TValue>(propertyInfo, valueSelector, this, settings, _currentConditions, _loggers);
            }
        }
        /// <inheritdoc/>
        public IValidationRuleConfigurator<TEntity, TError, PropertyValidationInfo, TPropertyValue> ForProperty<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> property, RuleSettings settings = RuleSettings.None)
        {
            using (_loggers.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                if (!property.TryExtractProperty(out var propertyInfo) || !typeof(TEntity).IsAssignableTo(propertyInfo.ReflectedType))
                {
                    throw new ArgumentException($"{nameof(property)} must select a property on {typeof(TEntity)}");
                }

                return new PropertyValidationRule<TEntity, TError, TPropertyValue, TPropertyValue>(propertyInfo, false, x => x, this, settings, _currentConditions, _loggers);
            }
        }
        /// <inheritdoc/>
        public IValidationRuleConfigurator<TEntity, TError, PropertyValidationInfo, TValue> ForProperty<TPropertyValue, TValue>(Expression<Func<TEntity, TPropertyValue>> property, Func<TPropertyValue, TValue> valueSelector, RuleSettings settings = RuleSettings.None)
        {
            using (_loggers.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                valueSelector.ValidateArgument(nameof(valueSelector));
                if (!property.TryExtractProperty(out var propertyInfo) || !propertyInfo.ReflectedType.IsAssignableTo<TEntity>())
                {
                    throw new ArgumentException($"{nameof(property)} must select a property on {typeof(TEntity)}");
                }

                return new PropertyValidationRule<TEntity, TError, TPropertyValue, TValue>(propertyInfo, true, valueSelector, this, settings, _currentConditions, _loggers);
            }
        }
        /// <inheritdoc/>
        public IValidationRuleConfigurator<TEntity, TError, NullValidationInfo, TEntity> ForSource(RuleSettings settings = RuleSettings.None)
        {
            using (_loggers.TraceMethod(this))
            {
                return ForSource(x => x, settings);
            }
        }
        /// <inheritdoc/>
        public IValidationRuleConfigurator<TEntity, TError, NullValidationInfo, TValue> ForSource<TValue>(Func<TEntity, TValue> valueSelector, RuleSettings settings = RuleSettings.None)
        {
            using (_loggers.TraceMethod(this))
            {
                valueSelector.ValidateArgument(nameof(valueSelector));
                
                return new SourceValidationRule<TEntity, TError, TValue>(valueSelector, this, settings, _currentConditions, _loggers);
            }
        }
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TError> ValidateWhen(AsyncPredicate<IValidationRuleContext<TEntity, object>> condition, Action<IValidationConfigurator<TEntity, TError>> configurator)
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
        public IValidationConfigurator<TEntity, TError> ValidateWhen<TContext>(AsyncPredicate<IValidationRuleContext<TEntity, TContext>> condition, Action<IValidationConfigurator<TEntity, TContext, TError>> configurator)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                configurator.ValidateArgument(nameof(configurator));

                AsyncPredicate<IValidationRuleContext<TEntity, object>> contextCondition = x => {
                    var context = new ValidationRuleContext<TEntity, TContext>(x.Source, x.Context, x.ElementIndex, x.Parents);
                    return condition(context);
                };

                using (new ScopedAction(() => _currentConditions.Add(contextCondition), () => _currentConditions.Remove(contextCondition)))
                {
                    configurator(new EntityValidator<TEntity, TContext, TError>(false, this));
                }

                return this;
            }
        }
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TError> ValidateWhen(AsyncPredicate<IValidationRuleContext<TEntity, object>> condition)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));

                _currentConditions.Add(condition);

                return this;
            }
        }
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TError> ValidateWhen<TContext>(AsyncPredicate<IValidationRuleContext<TEntity, TContext>> condition)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));

                _currentConditions.Add(x => condition(new ValidationRuleContext<TEntity, TContext>(x.Source, x.Context, x.ElementIndex, x.Parents)));

                return this;
            }
        }

        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TError> ValidateWhen(Predicate<IValidationRuleContext<TEntity, object>> condition, Action<IValidationConfigurator<TEntity, TError>> configurator)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                configurator.ValidateArgument(nameof(configurator));

                return ValidateWhen(x => Task.FromResult(condition(x)), configurator);
            }
        }
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TError> ValidateWhen<TContext>(Predicate<IValidationRuleContext<TEntity, TContext>> condition, Action<IValidationConfigurator<TEntity, TContext, TError>> configurator)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                configurator.ValidateArgument(nameof(configurator));

                return ValidateWhen<TContext>(x => Task.FromResult(condition(x)), configurator);
            }
        }
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TError> ValidateWhen(Predicate<IValidationRuleContext<TEntity, object>> condition)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));

                return ValidateWhen(x => Task.FromResult(condition(x)));
            }
        }
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TError> ValidateWhen<TContext>(Predicate<IValidationRuleContext<TEntity, TContext>> condition)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));

                return ValidateWhen<TContext>(x => Task.FromResult(condition(x)));
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
        internal override async Task<TError[]> Validate(object objectToValidate, object context, int? elementIndex = null, Parent[] parents = null)
        {
            using (_loggers.TraceMethod(this))
            {
                objectToValidate.ValidateArgument(x => CanValidate(objectToValidate, context, elementIndex, parents), x => new NotSupportedException($"Current validator cannot validate <{(objectToValidate == null ? "null references" : objectToValidate.GetType().ToString())}>"));

                var typedObjectToValidate = objectToValidate.Cast<TEntity>();
                var validationRuleContext = new ValidationRuleContext<TEntity, object>(typedObjectToValidate, context, elementIndex, parents);
                var errors = new List<TError>();

                foreach(var rule in _rules)
                {
                    if(await rule.CanValidate(validationRuleContext))
                    {
                        try
                        {
                            var validationErrors = await rule.Validate(typedObjectToValidate, context, elementIndex, parents);

                            if (validationErrors.HasValue())
                            {
                                errors.AddRange(validationErrors);
                            }
                        }
                        catch(Exception ex)
                        {
                            if (rule.IgnoreExceptions)
                            {
                                _loggers.LogException(LogLevel.Warning, $"Rule has ignore exception enabled. Ignoring", ex);
                            }
                            else
                            {
                                throw;
                            }
                        }
                        
                    }
                }

                return errors.ToArray();
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
        internal abstract Task<TError[]> Validate(object objectToValidate, object context, int? elementIndex = null, Parent[] parents = null);
        #endregion
    }
}
