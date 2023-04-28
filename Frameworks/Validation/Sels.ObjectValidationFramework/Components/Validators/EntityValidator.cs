using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Extensions.Reflection;
using Sels.ObjectValidationFramework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using Sels.Core.Components.Scope;
using System.Linq.Expressions;
using Sels.Core.Extensions.Logging;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;
using Sels.ObjectValidationFramework.Configurators;
using Sels.ObjectValidationFramework.Target;
using Sels.ObjectValidationFramework.Profile;
using Sels.ObjectValidationFramework.Validators.Builder;

namespace Sels.ObjectValidationFramework.Validators
{
    /// <summary>
    /// Validator that allows for the creation of validation rules for type <typeparamref name="TEntity"/> and using the rules to validate instances of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of object to validate</typeparam>
    /// <typeparam name="TContext">The base context used for all validation rules created using the current instance</typeparam>
    /// <typeparam name="TError">Type of validation error that the validator returns</typeparam>
    internal class EntityValidator<TEntity, TContext, TError> : EntityValidator<TError>, IValidationConfigurator<TEntity, TContext, TError>
    {
        // Fields
        private readonly TargetExecutionOptions _defaultSettings;
        private readonly List<BaseValidationTarget<TEntity, TContext, TError>> _targets = new List<BaseValidationTarget<TEntity, TContext, TError>>();
        private readonly List<AsyncPredicate<IValidationRuleContext<TEntity, TContext>>> _currentConditions = new List<AsyncPredicate<IValidationRuleContext<TEntity, TContext>>>();
        private readonly Action<EntityValidator<TError>> _onValidatorCreatedHandler;

        /// <inheritdoc cref="EntityValidator{TEntity, TContext, TError}"/>
        /// <param name="onValidatorCreatedHandler">Delegate called when the current validator creates another (sub) validator</param>
        /// <param name="defaultSettings">The default settings to use for all created validation targets</param>
        /// <param name="logger"><inheritdoc cref="EntityValidator{TError}._logger"/></param>
        internal EntityValidator(Action<EntityValidator<TError>> onValidatorCreatedHandler, TargetExecutionOptions defaultSettings, ILogger logger = null) : base(logger)
        {
            _defaultSettings = defaultSettings;
            _onValidatorCreatedHandler = onValidatorCreatedHandler.ValidateArgument(nameof(onValidatorCreatedHandler));
        }
       
        /// <summary>
        /// Adds a new validation target to the current validator containing validation rules.
        /// </summary>
        /// <param name="target">The validation target to add</param>
        internal void AddTargetRule(BaseValidationTarget<TEntity, TContext, TError> target)
        {
            using (_logger.TraceMethod(this))
            {
                target.ValidateArgument(nameof(target));

                _targets.Add(target);
            }
        }

        #region Configuration
        #region Target
        /// <inheritdoc/>
        public IValidationTargetRootConfigurator<TEntity, TError, TContext, CollectionPropertyValidationInfo, TElement> ForElements<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> property, TargetExecutionOptions settings = TargetExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));

                return ForElements(property, x => x, settings);
            }
        }
        /// <inheritdoc/>
        public IValidationTargetRootConfigurator<TEntity, TError, TContext, CollectionPropertyValidationInfo, TValue> ForElements<TElement, TValue>(Expression<Func<TEntity, IEnumerable<TElement>>> property, Func<TElement, TValue> valueSelector, TargetExecutionOptions settings = TargetExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                valueSelector.ValidateArgument(nameof(valueSelector));
                if (!property.TryExtractProperty(out var propertyInfo) || !propertyInfo.ReflectedType.IsAssignableTo<TEntity>())
                {
                    throw new ArgumentException($"{nameof(property)} must select a property on {typeof(TEntity)}");
                }

                return new CollectionPropertyValidationTarget<TEntity, TError, TContext, TElement, TValue>(propertyInfo, property.Compile(), valueSelector, this, _defaultSettings | settings, _currentConditions, _logger);
            }
        }
        /// <inheritdoc/>
        public IValidationTargetRootConfigurator<TEntity, TError, TContext, PropertyValidationInfo, TPropertyValue> ForProperty<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> property, TargetExecutionOptions settings = TargetExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                if (!property.TryExtractProperty(out var propertyInfo) || !typeof(TEntity).IsAssignableTo(propertyInfo.ReflectedType))
                {
                    throw new ArgumentException($"{nameof(property)} must select a property on {typeof(TEntity)}");
                }

                return new PropertyValidationTarget<TEntity, TError, TContext, TPropertyValue, TPropertyValue>(propertyInfo, false, property.Compile(), x => x, this, _defaultSettings | settings, _currentConditions, _logger);
            }
        }
        /// <inheritdoc/>
        public IValidationTargetRootConfigurator<TEntity, TError, TContext, PropertyValidationInfo, TValue> ForProperty<TPropertyValue, TValue>(Expression<Func<TEntity, TPropertyValue>> property, Func<TPropertyValue, TValue> valueSelector, TargetExecutionOptions settings = TargetExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                valueSelector.ValidateArgument(nameof(valueSelector));
                if (!property.TryExtractProperty(out var propertyInfo) || !propertyInfo.ReflectedType.IsAssignableTo<TEntity>())
                {
                    throw new ArgumentException($"{nameof(property)} must select a property on {typeof(TEntity)}");
                }

                return new PropertyValidationTarget<TEntity, TError, TContext, TPropertyValue, TValue>(propertyInfo, true, property.Compile(), valueSelector, this, _defaultSettings | settings, _currentConditions, _logger);
            }
        }
        /// <inheritdoc/>
        public IValidationTargetRootConfigurator<TEntity, TError, TContext, NullValidationInfo, TEntity> ForSource(TargetExecutionOptions settings = TargetExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                return ForSource(x => x, settings);
            }
        }
        /// <inheritdoc/>
        public IValidationTargetRootConfigurator<TEntity, TError, TContext, NullValidationInfo, TValue> ForSource<TValue>(Func<TEntity, TValue> valueSelector, TargetExecutionOptions settings = TargetExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                valueSelector.ValidateArgument(nameof(valueSelector));

                return new SourceValidationTarget<TEntity, TError, TContext, TValue>(valueSelector, this, _defaultSettings | settings, _currentConditions, _logger);
            }
        }
        #endregion

        #region Condition
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TContext, TError> ValidateWhen(AsyncPredicate<IValidationRuleContext<TEntity, TContext>> condition, Action<IValidationConfigurator<TEntity, TContext, TError>> configurator)
        {
            using (_logger.TraceMethod(this))
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
        public IValidationConfigurator<TEntity, TContext, TError> ValidateWhen<TNewContext>(AsyncPredicate<IValidationRuleContext<TEntity, TNewContext>> condition, Action<IValidationConfigurator<TEntity, TNewContext, TError>> configurator, bool contextRequired = true) where TNewContext : TContext
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                configurator.ValidateArgument(nameof(configurator));

                AsyncPredicate<IValidationRuleContext<TEntity, TContext>> contextCondition = x => {
                    var context = new ValidationRuleContext<TEntity, TNewContext>(x.Source, x.Context.CastToOrDefault<TNewContext>(), x.ElementIndex, x.Parents);
                    if (contextRequired && !context.HasContext) return Task.FromResult(false);
                    return condition(context);
                };

                var validator = new EntityValidator<TEntity, TNewContext, TError>(_onValidatorCreatedHandler, _defaultSettings, _logger);
                validator.ValidateWhen(x =>
                {
                    if (contextRequired && !x.HasContext) return Task.FromResult(false);
                    return condition(x);
                }, configurator);


                _onValidatorCreatedHandler(validator);

                return this;
            }
        }

        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TContext, TError> ValidateWhen(Predicate<IValidationRuleContext<TEntity, TContext>> condition, Action<IValidationConfigurator<TEntity, TContext, TError>> configurator)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                configurator.ValidateArgument(nameof(configurator));

                return ValidateWhen(x => Task.FromResult(condition(x)), configurator);
            }
        }
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TContext, TError> ValidateWhen<TNewContext>(Predicate<IValidationRuleContext<TEntity, TNewContext>> condition, Action<IValidationConfigurator<TEntity, TNewContext, TError>> configurator, bool contextRequired = true) where TNewContext : TContext
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                configurator.ValidateArgument(nameof(configurator));

                return ValidateWhen<TNewContext>(x => Task.FromResult(condition(x)), configurator, contextRequired);
            }
        }
        
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TContext, TError> ValidateNextWhen(AsyncPredicate<IValidationRuleContext<TEntity, TContext>> condition)
        {
            condition.ValidateArgument(nameof(condition));
            _currentConditions.Add(condition);
            return this;
        }
        /// <inheritdoc/>
        public IValidationConfigurator<TEntity, TContext, TError> ValidateNextWhen(Predicate<IValidationRuleContext<TEntity, TContext>> condition)
        {
            condition.ValidateArgument(nameof(condition));
            return ValidateNextWhen(x => Task.FromResult(condition(x)));
        }
        
        /// <inheritdoc/>
        public ISwitchRootConditionConfigurator<TEntity, TError, TContext, TContext, TValue> Switch<TValue>(Func<TEntity, TValue> valueSelector, Predicate<IValidationRuleContext<TEntity, TContext>> condition = null)
        {
            valueSelector.ValidateArgument(nameof(valueSelector));

            return Switch(valueSelector, condition != null ? x => Task.FromResult(condition(x)) : (AsyncPredicate<IValidationRuleContext<TEntity, TContext>>)null);
        }
        /// <inheritdoc/>
        public ISwitchRootConditionConfigurator<TEntity, TError, TContext, TContext, TValue> Switch<TValue>(Func<TEntity, TValue> valueSelector, AsyncPredicate<IValidationRuleContext<TEntity, TContext>> condition = null)
        {
            valueSelector.ValidateArgument(nameof(valueSelector));

            // Set on exit action to handle the condition for the current builder
            Action onExitAction;
            if (condition != null)
            {
                _currentConditions.Add(condition);
                onExitAction = () => _currentConditions.Remove(condition);
            }
            else
            {
                onExitAction = () => { };
            }

            return new ValidatorSwitchConditionBuilder<TEntity, TError, TContext, TContext, TValue>(this, this, valueSelector, onExitAction);
        }

        /// <inheritdoc/>
        public ISwitchRootConditionConfigurator<TEntity, TError, TContext, TNewContext, TValue> Switch<TNewContext, TValue>(Func<TEntity, TValue> valueSelector, Predicate<IValidationRuleContext<TEntity, TNewContext>> condition = null, bool contextRequired = true)
        where TNewContext : TContext
        {
            valueSelector.ValidateArgument(nameof(valueSelector));

            return Switch(valueSelector, condition != null ? x => Task.FromResult(condition(x)) : (AsyncPredicate<IValidationRuleContext<TEntity, TNewContext>>)null, contextRequired);
        }
        /// <inheritdoc/>
        public ISwitchRootConditionConfigurator<TEntity, TError, TContext, TNewContext, TValue> Switch<TNewContext, TValue>(Func<TEntity, TValue> valueSelector, AsyncPredicate<IValidationRuleContext<TEntity, TNewContext>> condition = null, bool contextRequired = true)
        where TNewContext : TContext
        {
            valueSelector.ValidateArgument(nameof(valueSelector));

            // Create validator that targets new context and add condition if needed
            var targetBuilder = new EntityValidator<TEntity, TNewContext, TError>(_onValidatorCreatedHandler, _defaultSettings, _logger);
            targetBuilder.ValidateNextWhen(x =>
            {
                if (contextRequired && !x.HasContext) return Task.FromResult(false);
                return condition != null ? condition(x) : Task.FromResult(true);
            });
            
            // Add the validator when the switch builder exists
            Action onExitAction = () => _onValidatorCreatedHandler(targetBuilder);
            

            return new ValidatorSwitchConditionBuilder<TEntity, TError, TContext, TNewContext, TValue>(this, targetBuilder, valueSelector, onExitAction);
        }
        #endregion
        #endregion

        #region Validation
        /// <inheritdoc/>
        internal override bool CanValidate(object objectToValidate, object context, int? elementIndex = null, Parent[] parents = null)
        {
            using (_logger.TraceMethod(this))
            {
                return objectToValidate != null && objectToValidate.IsAssignableTo<TEntity>();
            }
        }

        /// <inheritdoc/>
        internal override async Task<ValidationError<TError>[]> Validate(object objectToValidate, object context, int? elementIndex = null, Parent[] parents = null)
        {
            using (_logger.TraceMethod(this))
            {
                objectToValidate.ValidateArgument(x => CanValidate(objectToValidate, context, elementIndex, parents), x => new NotSupportedException($"Current validator cannot validate <{(objectToValidate == null ? "null references" : objectToValidate.GetType().ToString())}>"));

                var typedObjectToValidate = objectToValidate.CastTo<TEntity>();
                var castedContext = context.CastToOrDefault<TContext>();
                var validationRuleContext = new ValidationRuleContext<TEntity, TContext>(typedObjectToValidate, castedContext, elementIndex, parents);
                var errors = new List<ValidationError<TError>>();

                _logger.Log($"Executing all validation rules defined for object of type <{typeof(TEntity)}>");

                foreach(var target in _targets)
                {
                    if(await target.CanValidate(validationRuleContext).ConfigureAwait(false))
                    {
                        try
                        {
                            var validationErrors = await target.ValidateObject(typedObjectToValidate, castedContext, elementIndex, parents).ConfigureAwait(false);

                            if (validationErrors.HasValue())
                            {
                                errors.AddRange(validationErrors);
                            }
                        }
                        catch(Exception ex)
                        {
                            _logger.Log($"Validation target <{target.Identifier}> for object of type <{typeof(TEntity)}> threw an exception while executing it's validation rules", ex);
                            throw;
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
        /// <summary>
        /// Optional logger for tracing.
        /// </summary>
        protected readonly ILogger _logger;

        /// <inheritdoc cref="EntityValidator{TError}"/>
        /// <param name="logger"><inheritdoc cref="_logger"/></param>
        public EntityValidator(ILogger logger = null)
        {
            _logger = logger;
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
        internal abstract Task<ValidationError<TError>[]> Validate(object objectToValidate, object context, int? elementIndex = null, Parent[] parents = null);
        #endregion
    }
}
