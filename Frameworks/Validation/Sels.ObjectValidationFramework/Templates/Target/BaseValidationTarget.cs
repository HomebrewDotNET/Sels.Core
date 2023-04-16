using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.ObjectValidationFramework.Validators;
using System.Linq.Expressions;
using static Sels.Core.Delegates.Async;
using System.Threading.Tasks;
using Sels.ObjectValidationFramework.Configurators;
using Sels.Core.Extensions.Conversion;
using Sels.ObjectValidationFramework.Rules;
using Sels.ObjectValidationFramework.Profile;
using Sels.Core.Extensions.Linq;

namespace Sels.ObjectValidationFramework.Target
{
    /// <summary>
    /// Delegates calls to <see cref="IValidationConfigurator{TEntity, TContext, TError}"/>
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TContext">The type of the base context of the base validator</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    internal abstract class BaseValidationTarget<TEntity, TContext, TError> : IValidationConfigurator<TEntity, TContext, TError>
    {
        // Fields
        /// <summary>
        /// The validator tha was used to create the current target.
        /// </summary>
        protected readonly EntityValidator<TEntity, TContext, TError> _validator;
        /// <summary>
        /// Global conditions defined for all rules created for the current target.
        /// </summary>
        protected readonly AsyncPredicate<IValidationRuleContext<TEntity, TContext>>[] _globalConditions;
        /// <summary>
        /// Optional logger for tracing.
        /// </summary>
        protected readonly ILogger _logger;
        /// <summary>
        /// The execution settings for the current target.
        /// </summary>
        protected readonly TargetExecutionOptions _settings;

        // Properties
        /// <summary>
        /// Human readable name used to identify the current validation target. Is used for logging.
        /// </summary>
        public abstract string Identifier { get; }

        /// <inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}"/>.
        /// <param name="validator"><inheritdoc cref="_validator"/></param>
        /// <param name="settings"><inheritdoc cref="_settings"/></param>
        /// <param name="globalConditions"><inheritdoc cref="_globalConditions"/></param>
        /// <param name="logger"><inheritdoc cref="_logger"/></param>
        internal BaseValidationTarget(EntityValidator<TEntity, TContext, TError> validator, TargetExecutionOptions settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, TContext>>> globalConditions = null, ILogger logger = null)
        {
            _validator = validator.ValidateArgument(nameof(validator));
            _settings = settings;
            _logger = logger;

            _globalConditions = globalConditions.ToArrayOrDefault();
        }

        #region Validator
        /// <inhericdoc />
        public IValidationTargetRootConfigurator<TEntity, TError, TContext, CollectionPropertyValidationInfo, TElement> ForElements<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> property, TargetExecutionOptions settings = TargetExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));

                return _validator.ForElements(property, settings);
            }
        }
        /// <inhericdoc />
        public IValidationTargetRootConfigurator<TEntity, TError, TContext, CollectionPropertyValidationInfo, TValue1> ForElements<TElement, TValue1>(Expression<Func<TEntity, IEnumerable<TElement>>> property, Func<TElement, TValue1> valueSelector, TargetExecutionOptions settings = TargetExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                valueSelector.ValidateArgument(nameof(valueSelector));

                return _validator.ForElements(property, valueSelector, settings);
            }
        }
        /// <inhericdoc />
        public IValidationTargetRootConfigurator<TEntity, TError, TContext, PropertyValidationInfo, TPropertyValue> ForProperty<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> property, TargetExecutionOptions settings = TargetExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));

                return _validator.ForProperty(property, settings);
            }
        }
        /// <inhericdoc />
        public IValidationTargetRootConfigurator<TEntity, TError, TContext, PropertyValidationInfo, TValue1> ForProperty<TPropertyValue, TValue1>(Expression<Func<TEntity, TPropertyValue>> property, Func<TPropertyValue, TValue1> valueSelector, TargetExecutionOptions settings = TargetExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                valueSelector.ValidateArgument(nameof(valueSelector));

                return _validator.ForProperty(property, valueSelector, settings);
            }
        }
        /// <inhericdoc />
        public IValidationTargetRootConfigurator<TEntity, TError, TContext, NullValidationInfo, TEntity> ForSource(TargetExecutionOptions settings = TargetExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                return _validator.ForSource(settings);
            }
        }
        /// <inhericdoc />
        public IValidationTargetRootConfigurator<TEntity, TError, TContext, NullValidationInfo, TValue1> ForSource<TValue1>(Func<TEntity, TValue1> valueSelector, TargetExecutionOptions settings = TargetExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                valueSelector.ValidateArgument(nameof(valueSelector));

                return _validator.ForSource(valueSelector, settings);
            }
        }
        /// <inhericdoc />
        public IValidationConfigurator<TEntity, TContext, TError> ValidateWhen(Predicate<IValidationRuleContext<TEntity, TContext>> condition, Action<IValidationConfigurator<TEntity, TContext, TError>> configurator)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                configurator.ValidateArgument(nameof(configurator));

                return _validator.ValidateWhen(condition, configurator);
            }
        }
        /// <inhericdoc />
        public IValidationConfigurator<TEntity, TContext, TError> ValidateWhen<TNewContext>(Predicate<IValidationRuleContext<TEntity, TNewContext>> condition, Action<IValidationConfigurator<TEntity, TNewContext, TError>> configurator, bool contextRequired = true) where TNewContext : TContext
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                configurator.ValidateArgument(nameof(configurator));

                return _validator.ValidateWhen(condition, configurator, contextRequired);
            }
        }
        /// <inhericdoc />
        public IValidationConfigurator<TEntity, TContext, TError> ValidateWhen(AsyncPredicate<IValidationRuleContext<TEntity, TContext>> condition, Action<IValidationConfigurator<TEntity, TContext, TError>> configurator)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                configurator.ValidateArgument(nameof(configurator));

                return _validator.ValidateWhen(condition, configurator);
            }
        }
        /// <inhericdoc />
        public IValidationConfigurator<TEntity, TContext, TError> ValidateWhen<TNewContext>(AsyncPredicate<IValidationRuleContext<TEntity, TNewContext>> condition, Action<IValidationConfigurator<TEntity, TNewContext, TError>> configurator, bool contextRequired = true) where TNewContext : TContext
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                configurator.ValidateArgument(nameof(configurator));

                return _validator.ValidateWhen(condition, configurator);
            }
        }
        /// <inhericdoc />
        public IValidationConfigurator<TEntity, TContext, TError> ValidateNextWhen(AsyncPredicate<IValidationRuleContext<TEntity, TContext>> condition)
        {
            return ((IValidationConfigurator<TEntity, TContext, TError>)_validator).ValidateNextWhen(condition);
        }
        /// <inhericdoc />
        public IValidationConfigurator<TEntity, TContext, TError> ValidateNextWhen(Predicate<IValidationRuleContext<TEntity, TContext>> condition)
        {
            return ((IValidationConfigurator<TEntity, TContext, TError>)_validator).ValidateNextWhen(condition);
        }
        /// <inhericdoc />
        public ISwitchRootConditionConfigurator<TEntity, TError, TContext, TContext, TValue> Switch<TValue>(Func<TEntity, TValue> valueSelector, Predicate<IValidationRuleContext<TEntity, TContext>> condition = null)
        {
            return ((IValidationConfigurator<TEntity, TContext, TError>)_validator).Switch(valueSelector, condition);
        }
        /// <inhericdoc />
        public ISwitchRootConditionConfigurator<TEntity, TError, TContext, TContext, TValue> Switch<TValue>(Func<TEntity, TValue> valueSelector, AsyncPredicate<IValidationRuleContext<TEntity, TContext>> condition = null)
        {
            return ((IValidationConfigurator<TEntity, TContext, TError>)_validator).Switch(valueSelector, condition);
        }
        /// <inhericdoc />
        public ISwitchRootConditionConfigurator<TEntity, TError, TContext, TNewContext, TValue> Switch<TNewContext, TValue>(Func<TEntity, TValue> valueSelector, Predicate<IValidationRuleContext<TEntity, TNewContext>> condition = null, bool contextRequired = true) where TNewContext : TContext
        {
            return ((IValidationConfigurator<TEntity, TContext, TError>)_validator).Switch(valueSelector, condition, contextRequired);
        }
        /// <inhericdoc />
        public ISwitchRootConditionConfigurator<TEntity, TError, TContext, TNewContext, TValue> Switch<TNewContext, TValue>(Func<TEntity, TValue> valueSelector, AsyncPredicate<IValidationRuleContext<TEntity, TNewContext>> condition = null, bool contextRequired = true) where TNewContext : TContext
        {
            return ((IValidationConfigurator<TEntity, TContext, TError>)_validator).Switch(valueSelector, condition, contextRequired);
        }
        #endregion

        #region Validation
        /// <summary>
        /// If this validation rule is enabled by checking all global conditions against <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Context of the current <typeparamref name="TEntity"/> that is being validated</param>
        internal async Task<bool> CanValidate(IValidationRuleContext<TEntity, TContext> context)
        {
            using (_logger.TraceMethod(this))
            {
                if (_globalConditions.HasValue())
                {
                    foreach (var condition in _globalConditions)
                    {
                        if (!(await condition(context).ConfigureAwait(false)))
                        {
                            _logger.Debug($"{Identifier} is not allowed to run due to global condition");
                            return false;
                        };
                    }
                }

                return true;
            }
        }
        /// <summary>
        /// Validates <paramref name="objectToValidate"/> and returns all validation errors.
        /// </summary>
        /// <param name="objectToValidate">Object to validate</param>
        /// <param name="context">Optional context that can be supplied to a validation rule</param>
        /// <param name="elementIndex">Index of <paramref name="objectToValidate"/> if it was part of a collection. Will be null if it wasn't part of a collection</param>
        /// <param name="parents">Hierarchy of object if property fallthrough is enabled. The previous element is always the parent of the next element</param>
        /// <returns>All the validation errors for <paramref name="objectToValidate"/></returns>
        public async Task<ValidationError<TError>[]> ValidateObject(TEntity objectToValidate, TContext context, int? elementIndex = null, Parent[] parents = null)
        {
            using (_logger.TraceMethod(this))
            {
                objectToValidate.ValidateArgument(nameof(objectToValidate));
                _logger.Log($"Validation target <{Identifier}> executing validation rules");

                var errors = await Validate(objectToValidate, context, elementIndex, parents).ConfigureAwait(false);
                if (errors.HasValue()) errors.Execute(x => ModifyError(x));
                return errors;
            }
        }

        /// <summary>
        /// Validates <paramref name="objectToValidate"/> and returns all validation errors.
        /// </summary>
        /// <param name="objectToValidate">Object to validate</param>
        /// <param name="context">Optional context that can be supplied to a validation rule</param>
        /// <param name="elementIndex">Index of <paramref name="objectToValidate"/> if it was part of a collection. Will be null if it wasn't part of a collection</param>
        /// <param name="parents">Hierarchy of object if property fallthrough is enabled. The previous element is always the parent of the next element</param>
        /// <returns>All the validation errors for <paramref name="objectToValidate"/></returns>
        protected abstract Task<ValidationError<TError>[]> Validate(TEntity objectToValidate, TContext context, int? elementIndex = null, Parent[] parents = null);

        /// <summary>
        /// Optional method that can be used by validation target to modify any errors returned.
        /// </summary>
        /// <param name="error">The error to modify</param>
        protected virtual void ModifyError(ValidationError<TError> error)
        {

        }
        #endregion
    }
}
