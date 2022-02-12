using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.ObjectValidationFramework.Components;
using Sels.ObjectValidationFramework.Components.Rules;
using Sels.ObjectValidationFramework.Components.Validators;
using Sels.ObjectValidationFramework.Contracts.Rules;
using Sels.ObjectValidationFramework.Contracts.Validators;
using Sels.ObjectValidationFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.ObjectValidationFramework.Templates.Rules
{
    /// <summary>
    /// Template for creating a validator that implements <see cref="IValidationRuleConfigurator{TEntity, TError, TInfo, TContext, TValue}"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
    /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal abstract class BaseContextValidationRule<TEntity, TError, TInfo, TContext, TValue> : BaseValidationRule<TEntity, TError>, IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue>
    {
        // Fields
        protected readonly List<Predicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>>> _currentConditions = new List<Predicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>>>();
        protected readonly List<ValidationRuleValidator> _validationRules = new List<ValidationRuleValidator>();

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="validator">Validator to delegate <see cref="IValidationConfigurator{TEntity, TError}"/> calls to </param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <param name="globalConditions">Global conditions that all need to pass before any validation rules are allowed to run</param>
        /// <param name="loggers">Option loggers for logging</param>
        internal BaseContextValidationRule(EntityValidator<TEntity, TError> validator, RuleSettings settings, IEnumerable<Predicate<IValidationRuleContext<TEntity, object>>> globalConditions = null, IEnumerable<ILogger> loggers = null) : base(validator, settings, globalConditions, loggers)
        {
        }

        #region Rule Configurator
        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> ValidateWhen(Predicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>> condition)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));

                _currentConditions.Add(condition);

                return this;
            }
        }
        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> ValidIf(Predicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>> condition, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                errorConstructor.ValidateArgument(nameof(errorConstructor));

                var rule = new ValidationRuleValidator() { Validator = condition, ErrorContructor = errorConstructor };
                rule.Conditions.AddRange(_currentConditions);
                _validationRules.Add(rule);

                return this;
            }
        }
        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> InvalidIf(Predicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>> condition, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                errorConstructor.ValidateArgument(nameof(errorConstructor));

                return ValidIf(x => !condition(x), errorConstructor);
            }
        }
        #endregion

        /// <summary>
        /// Returns all enabled validators where all conditions pass for <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Context of the current <typeparamref name="TEntity"/> that is being validated</param>
        /// <returns>All enabled validators</returns>
        protected ValidationRuleValidator[] GetEnabledValidatorsFor(IValidationRuleContext<TEntity, TInfo, TContext, TValue> context)
        {
            using (_loggers.TraceMethod(this))
            {
                return _validationRules.Where(x => x.Conditions.All(c => c(context))).ToArray();
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
        protected ValidationRuleContext<TEntity, TInfo, TContext, TValue> CreateContext(TEntity objectToValidate, object context, int? elementIndex = null, Parent[] parents = null)
        {
            return new ValidationRuleContext<TEntity, TInfo, TContext, TValue>(CreateInfo(objectToValidate, context, elementIndex, parents), objectToValidate, context, elementIndex, parents);
        }

        /// <summary>
        /// Wrapper for the validation rule delegates.
        /// </summary>
        protected class ValidationRuleValidator
        {
            /// <summary>
            /// Predicates that tell when the rule is allowed to run.
            /// </summary>
            public List<Predicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>>> Conditions { get; } = new List<Predicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>>>();
            /// <summary>
            /// Predicate that tells if the <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is valid or not.
            /// </summary>
            public Predicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>> Validator { get; set; }
            /// <summary>
            /// Function that creates the error in case that the value is not valid.
            /// </summary>
            public Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> ErrorContructor { get; set; }

        }

        // Abstrations
        /// <summary>
        /// Creates the <typeparamref name="TInfo"/> for the <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}"/>.
        /// </summary>
        /// <param name="objectToValidate">Object to validate</param>
        /// <param name="context">Optional context that can be supplied to a validation rule.</param>
        /// <param name="elementIndex">Index of <paramref name="objectToValidate"/> if it was part of a collection. Will be null if it wasn't part of a collection.</param>
        /// <param name="parents">Hierarchy of object if property fallthrough is enabled. The previous element is always the parent of the next element.</param>
        /// <returns>The info object for the <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}"/></returns>
        protected abstract TInfo CreateInfo(TEntity objectToValidate, object context, int? elementIndex = null, Parent[] parents = null);
    }
}
