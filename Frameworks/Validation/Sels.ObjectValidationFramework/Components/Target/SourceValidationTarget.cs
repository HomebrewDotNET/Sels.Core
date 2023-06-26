using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.ObjectValidationFramework.Validators;
using System;
using System.Collections.Generic;
using static Sels.Core.Delegates.Async;
using Sels.ObjectValidationFramework.Rules;
using Sels.Core.Extensions.Reflection;
using Sels.ObjectValidationFramework.Profile;

namespace Sels.ObjectValidationFramework.Target
{
    /// <summary>
    /// Validation rule for validating values selected from the source entity of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
    /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal class SourceValidationTarget<TEntity, TError, TBaseContext, TTargetContext, TValue> : BaseSingleValueValidationTarget<TEntity, TError, TBaseContext, NullValidationInfo, TTargetContext, TValue> where TTargetContext : TBaseContext
    {
        // Fields
        private readonly Func<TEntity, TValue> _valueSelector;

        // Properties
        public override string Identifier => $"Source.{typeof(TEntity).GetDisplayName(false)}";

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="valueSelector">Selects the value to validate from an instance of <typeparamref name="TEntity"/></param>
        /// <param name="validator"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._validator"/></param>
        /// <param name="settings"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._settings"/></param>
        /// <param name="globalConditions"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._globalConditions"/></param>
        /// <param name="logger"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._logger"/></param>
        public SourceValidationTarget(Func<TEntity, TValue> valueSelector, EntityValidator<TEntity, TBaseContext, TError> validator, TargetExecutionOptions settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, TBaseContext>>> globalConditions = null, ILogger logger = null) : base(validator, settings, globalConditions, logger)
        {
            _valueSelector = valueSelector.ValidateArgument(nameof(valueSelector));
        }

        /// <inheritdoc/>
        protected override NullValidationInfo CreateInfo(TEntity objectToValidate, TTargetContext context, int? elementIndex = null, Parent[] parents = null)
        {
            using (_logger.TraceMethod(this))
            {
                return NullValidationInfo.Instance;
            }
        }


        /// <inheritdoc/>
        protected override bool TryGetValueToValidate(TEntity objectToValidate, out TValue value)
        {
            using (_logger.TraceMethod(this))
            {
                // Source object can't be null
                value = _valueSelector(objectToValidate);
                return true;
            }
        }
        /// <inheritdoc/>
        protected override string GetDisplayNameFor(ValidationRuleContext<TEntity, NullValidationInfo, TTargetContext, TValue> context, bool includeParents) => context.GetFullDisplayName(includeParents);
    }

    /// <summary>
    /// Validation rule for validating values selected from the source entity of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TContext">Type of the validation context used by the current validator</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal class SourceValidationTarget<TEntity, TError, TContext, TValue> : BaseContextlessValidationTarget<TEntity, TError, TContext, NullValidationInfo, TValue>
    {
        // Fields
        private readonly Func<TEntity, TValue> _valueSelector;

        // Properties
        public override string Identifier => $"Source.{typeof(TEntity).GetDisplayName(false)}";

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="valueSelector">Selects the value to validate from an instance of <typeparamref name="TEntity"/></param>
        /// <param name="validator"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._validator"/></param>
        /// <param name="settings"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._settings"/></param>
        /// <param name="globalConditions"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._globalConditions"/></param>
        /// <param name="logger"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._logger"/></param>
        public SourceValidationTarget(Func<TEntity, TValue> valueSelector, EntityValidator<TEntity, TContext, TError> validator, TargetExecutionOptions settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, TContext>>> globalConditions = null, ILogger logger = null) : base(validator, settings, globalConditions, logger)
        {
            _valueSelector = valueSelector.ValidateArgument(nameof(valueSelector));
        }

        /// <inheritdoc/>
        protected override BaseContextValidationTarget<TEntity, TError, TContext, NullValidationInfo, TNewContext, TValue> CreateNewConfigurator<TNewContext>()
        {
            using (_logger.TraceMethod(this))
            {
                return new SourceValidationTarget<TEntity, TError, TContext, TNewContext, TValue>(_valueSelector, _validator, _settings, _globalConditions, _logger);
            }
        }
    }
}
