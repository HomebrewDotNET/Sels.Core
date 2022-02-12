using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.ObjectValidationFramework.Components;
using Sels.ObjectValidationFramework.Components.Validators;
using Sels.ObjectValidationFramework.Contracts.Rules;
using Sels.ObjectValidationFramework.Contracts.Validators;
using Sels.ObjectValidationFramework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.ObjectValidationFramework.Templates.Rules
{
    /// <summary>
    /// Template for creating a validator that implements <see cref="IValidationRuleConfigurator{TEntity, TError, TInfo, TValue}"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal abstract class BaseContextlessValidationRule<TEntity, TError, TInfo, TValue> : BaseValidationRule<TEntity, TError>, IValidationRuleConfigurator<TEntity, TError, TInfo, TValue>
    {
        // Fields
        protected readonly RuleSettings _settings;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="validator">Validator to delegate <see cref="IValidationConfigurator{TEntity, TError}"/> calls to </param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <param name="globalConditions">Global conditions that all need to pass before any validation rules are allowed to run</param>
        /// <param name="loggers">Option loggers for logging</param>
        internal BaseContextlessValidationRule(EntityValidator<TEntity, TError> validator, RuleSettings settings, IEnumerable<Predicate<IValidationRuleContext<TEntity, object>>> globalConditions = null, IEnumerable<ILogger> loggers = null) : base(validator, settings, globalConditions, loggers)
        {
            _settings = settings;
        }

        #region Rule Configurator
        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, TInfo, object, TValue> InvalidIf(Predicate<IValidationRuleContext<TEntity, TInfo, object, TValue>> condition, Func<IValidationRuleContext<TEntity, TInfo, object, TValue>, TError> errorConstructor)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                errorConstructor.ValidateArgument(nameof(errorConstructor));

                return CreateNewConfiguratorAndRegister<object>().InvalidIf(condition, errorConstructor);
            }
        }

        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, TInfo, object, TValue> ValidateWhen(Predicate<IValidationRuleContext<TEntity, TInfo, object, TValue>> condition)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));

                return CreateNewConfiguratorAndRegister<object>().ValidateWhen(condition);
            }
        }

        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, TInfo, object, TValue> ValidIf(Predicate<IValidationRuleContext<TEntity, TInfo, object, TValue>> condition, Func<IValidationRuleContext<TEntity, TInfo, object, TValue>, TError> errorConstructor)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                errorConstructor.ValidateArgument(nameof(errorConstructor));

                return CreateNewConfiguratorAndRegister<object>().ValidIf(condition, errorConstructor);
            }
        }

        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> WithContext<TContext>()
        {
            using (_loggers.TraceMethod(this))
            {
                return CreateNewConfiguratorAndRegister<TContext>();
            }
        }
        #endregion

        /// <inheritdoc/>
        internal override TError[] Validate(TEntity objectToValidate, object context, int? elementIndex = null, Parent[] parents = null)
        {
            return null;
        }

        private IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> CreateNewConfiguratorAndRegister<TContext>()
        {
            using (_loggers.TraceMethod(this))
            {
                var instance = CreateNewConfigurator<TContext>();
                instance.ValidateArgument(x => x != null, x => new InvalidOperationException($"{nameof(CreateNewConfigurator)} must return an instance"));

                _validator.AddNewRule(instance);

                return instance;
            }
        }

        /// <summary>
        /// Creates a new instance of the current validation rule but with a context of <typeparamref name="TContext"/>.
        /// </summary>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <returns>A new validation rule with context of <typeparamref name="TContext"/></returns>
        protected abstract BaseContextValidationRule<TEntity, TError, TInfo, TContext, TValue> CreateNewConfigurator<TContext>();
    }
}
