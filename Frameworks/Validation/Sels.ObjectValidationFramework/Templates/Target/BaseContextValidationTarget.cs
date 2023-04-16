using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.ObjectValidationFramework.Configurators;
using Sels.ObjectValidationFramework.Profile;
using Sels.ObjectValidationFramework.Rules;
using Sels.ObjectValidationFramework.Validators;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.ObjectValidationFramework.Target
{
    /// <summary>
    /// Template for creating a validator that implements <see cref="IValidationTargetConfigurator{TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue}"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
    /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
    /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal abstract class BaseContextValidationTarget<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> : BaseValidationTarget<TEntity, TBaseContext, TError>, IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> where TTargetContext : TBaseContext
    {
        // Fields
        protected readonly List<AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>>> _currentConditions = new List<AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>>>();
        protected readonly List<ValidationRule> _validationRules = new List<ValidationRule>();

        /// <inheritdoc cref="BaseContextValidationTarget{TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue}"/>.
        /// <param name="validator"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._validator"/></param>
        /// <param name="settings"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._settings"/></param>
        /// <param name="globalConditions"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._globalConditions"/></param>
        /// <param name="logger"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._logger"/></param>
        internal BaseContextValidationTarget(EntityValidator<TEntity, TBaseContext, TError> validator, TargetExecutionOptions settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, TBaseContext>>> globalConditions = null, ILogger logger = null) : base(validator, settings, globalConditions, logger)
        {
            if (settings.HasFlag(TargetExecutionOptions.WithSuppliedContext)) _ = NextWhen(x => x.HasContext);
        }

        #region Rule Configurator
        /// <inhericdoc />
        public IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> NextWhen(AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));

                _currentConditions.Add(condition);

                return this;
            }
        }
        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> ValidIf(AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition, AsyncFunc<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                errorConstructor.ValidateArgument(nameof(errorConstructor));

                var rule = new ValidationRule(this) { Validator = condition, ErrorContructor = errorConstructor };
                rule.Conditions.AddRange(_currentConditions);
                _validationRules.Add(rule);

                return rule;
            }
        }
        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> InvalidIf(AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition, AsyncFunc<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                errorConstructor.ValidateArgument(nameof(errorConstructor));

                return ValidIf(async x => !(await condition(x).ConfigureAwait(false)), errorConstructor);
            }
        }

        /// <inhericdoc />
        public IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> NextWhen(Predicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));

                return NextWhen(x => Task.FromResult(condition(x)));
            }
        }
        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> ValidIf(Predicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                errorConstructor.ValidateArgument(nameof(errorConstructor));

                return ValidIf(x => Task.FromResult(condition(x)), x => Task.FromResult(errorConstructor(x)));
            }
        }
        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> InvalidIf(Predicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                errorConstructor.ValidateArgument(nameof(errorConstructor));

                return InvalidIf(x => Task.FromResult(condition(x)), x => Task.FromResult(errorConstructor(x)));
            }
        }
        #endregion

        /// <summary>
        /// Returns all enabled validators where all conditions pass for <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Context of the current <typeparamref name="TEntity"/> that is being validated</param>
        /// <returns>All enabled validators</returns>
        protected async Task<ValidationRule[]> GetEnabledValidatorsFor(IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue> context)
        {
            using (_logger.TraceMethod(this))
            {
                var validators = new List<ValidationRule>();

                foreach(var validationRule in _validationRules)
                {
                    bool canAdd = true;
                    foreach(var condition in validationRule.Conditions)
                    {
                        if (!(await condition(context).ConfigureAwait(false))) {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                    {
                        validators.Add(validationRule);
                    }
                }

                _logger.Debug($"Got <{validators.Count}> validation rules that can be executed for <{Identifier}> using the supplied validation rule context");

                return validators.ToArray();
            }
        }

        /// <summary>
        /// Creates the validation rule context.
        /// </summary>
        /// <param name="objectToValidate">Object to validate</param>
        /// <param name="context">Optional context that can be supplied to a validation rule.</param>
        /// <param name="elementIndex">Index of <paramref name="objectToValidate"/> if it was part of a collection. Will be null if it wasn't part of a collection.</param>
        /// <param name="parents">Hierarchy of object if property fallthrough is enabled. The previous element is always the parent of the next element.</param>
        /// <returns>The validation rule context to use for validation</returns>
        protected ValidationRuleContext<TEntity, TInfo, TTargetContext, TValue> CreateContext(TEntity objectToValidate, TBaseContext context, int? elementIndex = null, Parent[] parents = null)
        {
            using(_logger.TraceMethod(this)) {
                objectToValidate.ValidateArgument(nameof(objectToValidate));
                var targetContext = context.CastOrDefault<TTargetContext>();
                var info = CreateInfo(objectToValidate, targetContext, elementIndex, parents);
                if (info == null) throw new InvalidOperationException($"Validation target <{Identifier}> returned null when requested to create the info object");

                return new ValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>(info, objectToValidate, targetContext, elementIndex, parents);
            }
        }

        // Abstrations
        /// <summary>
        /// Creates the <typeparamref name="TInfo"/> for the <see cref="IValidationRuleContext{TEntity, TInfo, TTargetContext, TValue}"/>.
        /// </summary>
        /// <param name="objectToValidate">Object to validate</param>
        /// <param name="context">Optional context that can be supplied to a validation rule.</param>
        /// <param name="elementIndex">Index of <paramref name="objectToValidate"/> if it was part of a collection. Will be null if it wasn't part of a collection.</param>
        /// <param name="parents">Hierarchy of object if property fallthrough is enabled. The previous element is always the parent of the next element.</param>
        /// <returns>The info object for the <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}"/></returns>
        protected abstract TInfo CreateInfo(TEntity objectToValidate, TTargetContext context, int? elementIndex = null, Parent[] parents = null);

        /// <summary>
        /// Represents a validation rule created for the current target.
        /// </summary>
        protected class ValidationRule : IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>
        {
            // Fields
            private readonly BaseContextValidationTarget<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> _parent;

            // Properties
            /// <summary>
            /// Predicates that tell when the rule is allowed to run.
            /// </summary>
            public List<AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>>> Conditions { get; } = new List<AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>>>();
            /// <summary>
            /// Predicate that tells if the <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is valid or not.
            /// </summary>
            public AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> Validator { get; set; }
            /// <summary>
            /// Function that creates the error in case that the value is not valid.
            /// </summary>
            public AsyncFunc<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> ErrorContructor { get; set; }

            /// <inheritdoc cref="ValidationRule"/>
            /// <param name="parent">The target that created the current rule</param>
            public ValidationRule(BaseContextValidationTarget<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> parent)
            {
                _parent = parent.ValidateArgument(nameof(parent));
            }
            /// <inheritdoc/>
            public IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> When(Predicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition)
            {
                using (_parent._logger.TraceMethod(this))
                {
                    condition.ValidateArgument(nameof(condition));

                    return When(x => Task.FromResult(condition(x)));
                }
            }
            /// <inheritdoc/>
            public IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> When(AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition)
            {
                using (_parent._logger.TraceMethod(this))
                {
                    condition.ValidateArgument(nameof(condition));
                    Conditions.Add(condition);
                    return this;
                }
            }

            #region Delegated
            /// <inheritdoc/>
            IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>.ValidIf(Predicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor) => _parent.ValidIf(condition, errorConstructor);
            /// <inheritdoc/>
            IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>.InvalidIf(Predicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor) => _parent.InvalidIf(condition, errorConstructor);
            /// <inheritdoc/>
            IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>.NextWhen(Predicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition) => _parent.NextWhen(condition);
            /// <inheritdoc/>
            IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>.ValidIf(AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition, AsyncFunc<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor) => _parent.ValidIf(condition, errorConstructor);
            /// <inheritdoc/>
            IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>.InvalidIf(AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition, AsyncFunc<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor) => _parent.InvalidIf(condition, errorConstructor);
            /// <inheritdoc/>
            IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>.NextWhen(AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>> condition) => _parent.NextWhen(condition);
            /// <inheritdoc/>
            IValidationTargetRootConfigurator<TEntity, TError, TBaseContext, NullValidationInfo, TEntity> IValidationConfigurator<TEntity, TBaseContext, TError>.ForSource(TargetExecutionOptions settings) => _parent.ForSource(settings);
            /// <inheritdoc/>
            IValidationTargetRootConfigurator<TEntity, TError, TBaseContext, NullValidationInfo, TValue1> IValidationConfigurator<TEntity, TBaseContext, TError>.ForSource<TValue1>(Func<TEntity, TValue1> valueSelector, TargetExecutionOptions settings) => _parent.ForSource<TValue1>(valueSelector, settings);
            /// <inheritdoc/>
            IValidationTargetRootConfigurator<TEntity, TError, TBaseContext, PropertyValidationInfo, TPropertyValue> IValidationConfigurator<TEntity, TBaseContext, TError>.ForProperty<TPropertyValue>(System.Linq.Expressions.Expression<Func<TEntity, TPropertyValue>> property, TargetExecutionOptions settings) => _parent.ForProperty(property, settings);
            /// <inheritdoc/>
            IValidationTargetRootConfigurator<TEntity, TError, TBaseContext, PropertyValidationInfo, TValue1> IValidationConfigurator<TEntity, TBaseContext, TError>.ForProperty<TPropertyValue, TValue1>(Expression<Func<TEntity, TPropertyValue>> property, Func<TPropertyValue, TValue1> valueSelector, TargetExecutionOptions settings) => _parent.ForProperty(property, valueSelector, settings);
            /// <inheritdoc/>
            IValidationTargetRootConfigurator<TEntity, TError, TBaseContext, CollectionPropertyValidationInfo, TElement> IValidationConfigurator<TEntity, TBaseContext, TError>.ForElements<TElement>(System.Linq.Expressions.Expression<Func<TEntity, IEnumerable<TElement>>> property, TargetExecutionOptions settings) => _parent.ForElements(property, settings);
            /// <inheritdoc/>
            IValidationTargetRootConfigurator<TEntity, TError, TBaseContext, CollectionPropertyValidationInfo, TValue1> IValidationConfigurator<TEntity, TBaseContext, TError>.ForElements<TElement, TValue1>(Expression<Func<TEntity, IEnumerable<TElement>>> property, Func<TElement, TValue1> valueSelector, TargetExecutionOptions settings) => _parent.ForElements(property, valueSelector, settings);
            /// <inheritdoc/>
            IValidationConfigurator<TEntity, TBaseContext, TError> IValidationConfigurator<TEntity, TBaseContext, TError>.ValidateWhen(Predicate<IValidationRuleContext<TEntity, TBaseContext>> condition, Action<IValidationConfigurator<TEntity, TBaseContext, TError>> configurator) => _parent.ValidateWhen(condition, configurator);
            /// <inheritdoc/>
            IValidationConfigurator<TEntity, TBaseContext, TError> IValidationConfigurator<TEntity, TBaseContext, TError>.ValidateWhen<TNewContext>(Predicate<IValidationRuleContext<TEntity, TNewContext>> condition, Action<IValidationConfigurator<TEntity, TNewContext, TError>> configurator, bool contextRequired) => _parent.ValidateWhen(condition, configurator, contextRequired);
            /// <inheritdoc/>
            IValidationConfigurator<TEntity, TBaseContext, TError> IValidationConfigurator<TEntity, TBaseContext, TError>.ValidateWhen(AsyncPredicate<IValidationRuleContext<TEntity, TBaseContext>> condition, Action<IValidationConfigurator<TEntity, TBaseContext, TError>> configurator) => _parent.ValidateWhen(condition, configurator);
            /// <inheritdoc/>
            IValidationConfigurator<TEntity, TBaseContext, TError> IValidationConfigurator<TEntity, TBaseContext, TError>.ValidateWhen<TNewContext>(AsyncPredicate<IValidationRuleContext<TEntity, TNewContext>> condition, Action<IValidationConfigurator<TEntity, TNewContext, TError>> configurator, bool contextRequired) => _parent.ValidateWhen(condition, configurator, contextRequired);
            /// <inheritdoc/>
            public IValidationConfigurator<TEntity, TBaseContext, TError> ValidateNextWhen(AsyncPredicate<IValidationRuleContext<TEntity, TBaseContext>> condition) => _parent.ValidateNextWhen(condition);
            /// <inheritdoc/>
            public IValidationConfigurator<TEntity, TBaseContext, TError> ValidateNextWhen(Predicate<IValidationRuleContext<TEntity, TBaseContext>> condition) => _parent.ValidateNextWhen(condition);
            /// <inheritdoc/>
            public ISwitchRootConditionConfigurator<TEntity, TError, TBaseContext, TBaseContext, TValue1> Switch<TValue1>(Func<TEntity, TValue1> valueSelector, Predicate<IValidationRuleContext<TEntity, TBaseContext>> condition = null) => _parent.Switch(valueSelector, condition);
            /// <inheritdoc/>
            public ISwitchRootConditionConfigurator<TEntity, TError, TBaseContext, TBaseContext, TValue1> Switch<TValue1>(Func<TEntity, TValue1> valueSelector, AsyncPredicate<IValidationRuleContext<TEntity, TBaseContext>> condition = null) => _parent.Switch(valueSelector, condition);
            /// <inheritdoc/>
            public ISwitchRootConditionConfigurator<TEntity, TError, TBaseContext, TNewContext, TValue1> Switch<TNewContext, TValue1>(Func<TEntity, TValue1> valueSelector, Predicate<IValidationRuleContext<TEntity, TNewContext>> condition = default, bool contextRequired = true) where TNewContext : TBaseContext => _parent.Switch(valueSelector, condition, contextRequired);
            /// <inheritdoc/>
            public ISwitchRootConditionConfigurator<TEntity, TError, TBaseContext, TNewContext, TValue1> Switch<TNewContext, TValue1>(Func<TEntity, TValue1> valueSelector, AsyncPredicate<IValidationRuleContext<TEntity, TNewContext>> condition = default, bool contextRequired = true) where TNewContext : TBaseContext => _parent.Switch(valueSelector, condition, contextRequired);
            #endregion
        }
    }
}
