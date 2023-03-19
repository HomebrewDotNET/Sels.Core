using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.ObjectValidationFramework.Validators;
using Sels.ObjectValidationFramework.Contracts.Validators;
using System;
using System.Collections.Generic;
using static Sels.Core.Delegates.Async;

namespace Sels.ObjectValidationFramework.Rules
{
    /// <summary>
    /// Validation rule for validating values selected from the source entity of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal class SourceValidationRule<TEntity, TError, TContext, TValue> : BaseSingleValueValidationRule<TEntity, TError, NullValidationInfo, TContext, TValue>
    {
        // Fields
        private readonly Func<TEntity, TValue> _valueSelector;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="valueSelector">Selects the value to validate from an instance of <typeparamref name="TEntity"/></param>
        /// <param name="validator">Validator to delegate <see cref="IValidationConfigurator{TEntity, TError}"/> calls to </param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <param name="globalConditions">Global conditions that all need to pass before any validation rules are allowed to run</param>
        /// <param name="loggers">Option loggers for logging</param>
        public SourceValidationRule(Func<TEntity, TValue> valueSelector, EntityValidator<TEntity, TError> validator, RuleSettings settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, object>>> globalConditions = null, IEnumerable<ILogger> loggers = null) : base(validator, settings, globalConditions, loggers)
        {
            _valueSelector = valueSelector.ValidateArgument(nameof(valueSelector));
        }

        /// <inheritdoc/>
        protected override NullValidationInfo CreateInfo(TEntity objectToValidate, object context, int? elementIndex = null, Parent[] parents = null)
        {
            using (_loggers.TraceMethod(this))
            {
                return NullValidationInfo.Instance;
            }
        }


        /// <inheritdoc/>
        protected override bool TryGetValueToValidate(TEntity objectToValidate, out TValue value)
        {
            using (_loggers.TraceMethod(this))
            {
                value = default;
                if(objectToValidate != null)
                {
                    value = _valueSelector(objectToValidate);
                    return true;
                }

                return false;                
            }
        }
    }

    /// <summary>
    /// Validation rule for validating values selected from the source entity of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal class SourceValidationRule<TEntity, TError, TValue> : BaseContextlessValidationRule<TEntity, TError, NullValidationInfo, TValue>
    {
        // Fields
        private readonly Func<TEntity, TValue> _valueSelector;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="valueSelector">Selects the value to validate from an instance of <typeparamref name="TEntity"/></param>
        /// <param name="validator">Validator to delegate <see cref="IValidationConfigurator{TEntity, TError}"/> calls to </param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <param name="globalConditions">Global conditions that all need to pass before any validation rules are allowed to run</param>
        /// <param name="loggers">Option loggers for logging</param>
        public SourceValidationRule(Func<TEntity, TValue> valueSelector, EntityValidator<TEntity, TError> validator, RuleSettings settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, object>>> globalConditions = null, IEnumerable<ILogger> loggers = null) : base(validator, settings, globalConditions, loggers)
        {
            _valueSelector = valueSelector.ValidateArgument(nameof(valueSelector));
        }

        /// <inheritdoc/>
        protected override BaseContextValidationRule<TEntity, TError, NullValidationInfo, TContext, TValue> CreateNewConfigurator<TContext>()
        {
            using (_loggers.TraceMethod(this))
            {
                return new SourceValidationRule<TEntity, TError, TContext, TValue>(_valueSelector, _validator, _settings, _globalConditions, _loggers);
            }
        }
    }
}
