using Microsoft.Extensions.Logging;
using Sels.Core;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.ObjectValidationFramework.Configurators;
using Sels.ObjectValidationFramework.Profile;
using Sels.ObjectValidationFramework.Rules;
using Sels.ObjectValidationFramework.Validators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.ObjectValidationFramework.Target
{
    /// <summary>
    /// Template for creating a validator that implements <see cref="IValidationTargetRootConfigurator{TEntity, TError, TContext, TInfo, TValue}"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
    /// <typeparam name="TContext">Type of the validation context used by the current validator</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal abstract class BaseContextlessValidationTarget<TEntity, TError, TContext, TInfo, TValue> : BaseValidationTarget<TEntity, TContext, TError>, IValidationTargetRootConfigurator<TEntity, TError, TContext, TInfo, TValue>
    {
        /// <inheritdoc cref="BaseContextlessValidationTarget{TEntity, TError, TContext, TInfo, TValue}"/>
        /// <param name="validator"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._validator"/></param>
        /// <param name="settings"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._settings"/></param>
        /// <param name="globalConditions"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._globalConditions"/></param>
        /// <param name="logger"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._logger"/></param>
        internal BaseContextlessValidationTarget(EntityValidator<TEntity, TContext, TError> validator, TargetExecutionOptions settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, TContext>>> globalConditions = null, ILogger logger = null) : base(validator, settings, globalConditions, logger)
        {
        }

        #region Rule Configurator
        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, TContext, TInfo, TContext, TValue> InvalidIf(Predicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>> condition, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                errorConstructor.ValidateArgument(nameof(errorConstructor));

                return CreateNewConfiguratorAndRegister<TContext>().InvalidIf(condition, errorConstructor);
            }
        }

        /// <inhericdoc />
        public IValidationTargetConfigurator<TEntity, TError, TContext, TInfo, TContext, TValue> NextWhen(Predicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>> condition)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));

                return CreateNewConfiguratorAndRegister<TContext>().NextWhen(condition);
            }
        }

        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, TContext, TInfo, TContext, TValue> ValidIf(Predicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>> condition, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                errorConstructor.ValidateArgument(nameof(errorConstructor));

                return CreateNewConfiguratorAndRegister<TContext>().ValidIf(condition, errorConstructor);
            }
        }
        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, TContext, TInfo, TContext, TValue> ValidIf(Delegates.Async.AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>> condition, Delegates.Async.AsyncFunc<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                errorConstructor.ValidateArgument(nameof(errorConstructor));

                return CreateNewConfiguratorAndRegister<TContext>().ValidIf(condition, errorConstructor);
            }
        }
        /// <inhericdoc />
        public IValidationRuleConfigurator<TEntity, TError, TContext, TInfo, TContext, TValue> InvalidIf(Delegates.Async.AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>> condition, Delegates.Async.AsyncFunc<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                errorConstructor.ValidateArgument(nameof(errorConstructor));

                return CreateNewConfiguratorAndRegister<TContext>().InvalidIf(condition, errorConstructor);
            }
        }
        /// <inhericdoc />
        public IValidationTargetConfigurator<TEntity, TError, TContext, TInfo, TContext, TValue> NextWhen(Delegates.Async.AsyncPredicate<IValidationRuleContext<TEntity, TInfo, TContext, TValue>> condition)
        {
            using (_logger.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));

                return CreateNewConfiguratorAndRegister<TContext>().NextWhen(condition);
            }
        }

        /// <inhericdoc />
        public IValidationTargetConfigurator<TEntity, TError, TContext, TInfo, TNewContext, TValue> WithContext<TNewContext>() where TNewContext : TContext
        {
            using (_logger.TraceMethod(this))
            {
                var configurator = CreateNewConfiguratorAndRegister<TNewContext>();
                return configurator;
            }
        }
        #endregion

        /// <inheritdoc/>
        protected override Task<ValidationError<TError>[]> Validate(TEntity objectToValidate, TContext context, int? elementIndex = null, Parent[] parents = null)
        {
            // Current instance doesn't have any rules defined because we are still in the root configurator. Return null by default. If this method is being called then the configuration is wrong.
            _logger.Warning($"Validation was called on root configurator <{Identifier}>. This indicates that a target was created without any rules. Consider removing the target");
            return Task.FromResult<ValidationError<TError>[]>(null);
        }

        private IValidationTargetConfigurator<TEntity, TError, TContext, TInfo, TNewContext, TValue> CreateNewConfiguratorAndRegister<TNewContext>() where TNewContext : TContext
        {
            using (_logger.TraceMethod(this))
            {
                var instance = CreateNewConfigurator<TNewContext>();
                instance.ValidateArgument(x => x != null, x => new InvalidOperationException($"{nameof(CreateNewConfigurator)} must return an instance"));

                _logger.Debug($"Validation context for validation target <{Identifier}> is type <{typeof(TNewContext)}>");

                _validator.AddTargetRule(instance);

                return instance;
            }
        }

        /// <summary>
        /// Creates a new instance of the current validation target but with a context of <typeparamref name="TNewContext"/>.
        /// </summary>
        /// <typeparam name="TNewContext">The requested context type for validation rules created by the returned instance</typeparam>
        /// <returns>A new validation rule with context of type <typeparamref name="TNewContext"/></returns>
        protected abstract BaseContextValidationTarget<TEntity, TError, TContext, TInfo, TNewContext, TValue> CreateNewConfigurator<TNewContext>() where TNewContext : TContext;
    }
}
