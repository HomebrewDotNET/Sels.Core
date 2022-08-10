using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Components.Rules;
using Sels.ObjectValidationFramework.Contracts.Rules;
using Sels.ObjectValidationFramework.Contracts.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.ObjectValidationFramework.Components;
using Sels.ObjectValidationFramework.Components.Validators;
using System.Linq;
using System.Linq.Expressions;
using Sels.ObjectValidationFramework.Models;
using static Sels.Core.Delegates.Async;
using System.Threading.Tasks;

namespace Sels.ObjectValidationFramework.Templates.Rules
{
    /// <summary>
    /// Template that wraps the method calls on <see cref="IValidationConfigurator{TEntity, TError}"/> and adds abstractions for validating objects of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    internal abstract class BaseValidationRule<TEntity, TError> : IValidationConfigurator<TEntity, TError>
    {
        // Fields
        protected readonly EntityValidator<TEntity, TError> _validator;
        protected readonly List<AsyncPredicate<IValidationRuleContext<TEntity, object>>> _globalConditions = new List<AsyncPredicate<IValidationRuleContext<TEntity, object>>>();
        protected readonly IEnumerable<ILogger> _loggers;

        // Properties
        internal bool IgnoreExceptions { get; }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="validator">Validator to delegate <see cref="IValidationConfigurator{TEntity, TError}"/> calls to</param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <param name="globalConditions">Global conditions that all need to pass before any validation rules are allowed to run</param>
        /// <param name="loggers">Option loggers for logging</param>
        internal BaseValidationRule(EntityValidator<TEntity, TError> validator, RuleSettings settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, object>>> globalConditions = null, IEnumerable<ILogger> loggers = null)
        {
            _validator = validator.ValidateArgument(nameof(validator));
            _loggers = loggers;
            IgnoreExceptions = settings.HasFlag(RuleSettings.IgnoreExceptions);

            if (globalConditions.HasValue())
            {
                _globalConditions.AddRange(globalConditions);
            }
        }

        #region Validator
        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, CollectionPropertyValidationInfo, TElement> ForElements<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> property, RuleSettings settings = RuleSettings.None)
        {
            using (_loggers.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));

                return _validator.ForElements(property, settings);
            }
        }
        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, CollectionPropertyValidationInfo, TValue1> ForElements<TElement, TValue1>(Expression<Func<TEntity, IEnumerable<TElement>>> property, Func<TElement, TValue1> valueSelector, RuleSettings settings = RuleSettings.None)
        {
            using (_loggers.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                valueSelector.ValidateArgument(nameof(valueSelector));

                return _validator.ForElements(property, valueSelector, settings);
            }
        }
        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, PropertyValidationInfo, TPropertyValue> ForProperty<TPropertyValue>(Expression<Func<TEntity, TPropertyValue>> property, RuleSettings settings = RuleSettings.None)
        {
            using (_loggers.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));

                return _validator.ForProperty(property, settings);
            }
        }
        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, PropertyValidationInfo, TValue1> ForProperty<TPropertyValue, TValue1>(Expression<Func<TEntity, TPropertyValue>> property, Func<TPropertyValue, TValue1> valueSelector, RuleSettings settings = RuleSettings.None)
        {
            using (_loggers.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                valueSelector.ValidateArgument(nameof(valueSelector));

                return _validator.ForProperty(property, valueSelector, settings);
            }
        }
        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, NullValidationInfo, TEntity> ForSource(RuleSettings settings = RuleSettings.None)
        {
            using (_loggers.TraceMethod(this))
            {
                return _validator.ForSource(settings);
            }
        }

        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, NullValidationInfo, TValue1> ForSource<TValue1>(Func<TEntity, TValue1> valueSelector, RuleSettings settings = RuleSettings.None)
        {
            using (_loggers.TraceMethod(this))
            {
                valueSelector.ValidateArgument(nameof(valueSelector));

                return _validator.ForSource(valueSelector, settings);
            }
        }

        /// <inhericdoc />
        public IValidationConfigurator<TEntity, TError> ValidateWhen(Predicate<IValidationRuleContext<TEntity, object>> condition, Action<IValidationConfigurator<TEntity, TError>> configurator)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                configurator.ValidateArgument(nameof(configurator));

                return _validator.ValidateWhen(condition, configurator);
            }
        }

        /// <inhericdoc />
        public IValidationConfigurator<TEntity, TError> ValidateWhen<TContext>(Predicate<IValidationRuleContext<TEntity, TContext>> condition, Action<IValidationConfigurator<TEntity, TContext, TError>> configurator)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                configurator.ValidateArgument(nameof(configurator));

                return _validator.ValidateWhen(condition, configurator);
            }
        }
        /// <inhericdoc />
        public IValidationConfigurator<TEntity, TError> ValidateWhen(Predicate<IValidationRuleContext<TEntity, object>> condition)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));

                return _validator.ValidateWhen(condition);
            }
        }
        /// <inhericdoc />
        public IValidationConfigurator<TEntity, TError> ValidateWhen<TContext>(Predicate<IValidationRuleContext<TEntity, TContext>> condition)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));

                return _validator.ValidateWhen(condition);
            }
        }

        /// <inhericdoc />
        public IValidationConfigurator<TEntity, TError> ValidateWhen(AsyncPredicate<IValidationRuleContext<TEntity, object>> condition, Action<IValidationConfigurator<TEntity, TError>> configurator)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                configurator.ValidateArgument(nameof(configurator));

                return _validator.ValidateWhen(condition, configurator);
            }
        }

        /// <inhericdoc />
        public IValidationConfigurator<TEntity, TError> ValidateWhen<TContext>(AsyncPredicate<IValidationRuleContext<TEntity, TContext>> condition, Action<IValidationConfigurator<TEntity, TContext, TError>> configurator)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                configurator.ValidateArgument(nameof(configurator));

                return _validator.ValidateWhen(condition, configurator);
            }
        }
        /// <inhericdoc />
        public IValidationConfigurator<TEntity, TError> ValidateWhen(AsyncPredicate<IValidationRuleContext<TEntity, object>> condition)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));

                return _validator.ValidateWhen(condition);
            }
        }
        /// <inhericdoc />
        public IValidationConfigurator<TEntity, TError> ValidateWhen<TContext>(AsyncPredicate<IValidationRuleContext<TEntity, TContext>> condition)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));

                return _validator.ValidateWhen(condition);
            }
        }
        #endregion

        #region Validation
        /// <summary>
        /// If this validation rule is enabled by checking all global conditions against <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Context of the current <typeparamref name="TEntity"/> that is being validated</param>
        internal async Task<bool> CanValidate(IValidationRuleContext<TEntity, object> context)
        {
            using (_loggers.TraceMethod(this))
            {
                if (_globalConditions.HasValue())
                {
                    foreach(var condition in _globalConditions)
                    {
                        if (!(await condition(context))) return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Validates <paramref name="objectToValidate"/> and returns all validation errors.
        /// </summary>
        /// <param name="objectToValidate">Object to validate</param>
        /// <param name="context">Optional context that can be supplied to a validation rule.</param>
        /// <param name="elementIndex">Index of <paramref name="objectToValidate"/> if it was part of a collection. Will be null if it wasn't part of a collection.</param>
        /// <param name="parents">Hierarchy of object if property fallthrough is enabled. The previous element is always the parent of the next element.</param>
        /// <returns>All the validation errors for <paramref name="objectToValidate"/></returns>
        internal abstract Task<TError[]> Validate(TEntity objectToValidate, object context, int? elementIndex = null, Parent[] parents = null);
        #endregion
    }


}
