using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.ObjectValidationFramework.Profile;
using Sels.ObjectValidationFramework.Rules;
using Sels.ObjectValidationFramework.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.ObjectValidationFramework.Target
{
    /// <summary>
    /// Template for creating a validation target that validates multiple values (like a list, ...).
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
    /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
    /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal abstract class BaseMultiValueValidationTarget<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> : BaseContextValidationTarget<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> where TTargetContext : TBaseContext
    {
        /// <inheritdoc cref="BaseMultiValueValidationTarget{TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue}"/>.
        /// <param name="validator"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._validator"/></param>
        /// <param name="settings"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._settings"/></param>
        /// <param name="globalConditions"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._globalConditions"/></param>
        /// <param name="logger"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._logger"/></param>
        internal BaseMultiValueValidationTarget(EntityValidator<TEntity, TBaseContext, TError> validator, TargetExecutionOptions settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, TBaseContext>>> globalConditions = null, ILogger logger = null) : base(validator, settings, globalConditions, logger)
        {
        }

        /// <inheritdoc/>
        protected override async Task<ValidationError<TError>[]> Validate(TEntity objectToValidate, TBaseContext context, int? elementIndex = null, Parent[] parents = null)
        {
            using (_logger.TraceMethod(this))
            {
                objectToValidate.ValidateArgument(nameof(objectToValidate));
                var errors = new List<ValidationError<TError>>();

                _logger.TraceObject($"{Identifier} starting validation for ", objectToValidate);

                _logger.Debug($"{Identifier} creating validation context");
                var validationContext = CreateContext(objectToValidate, context, elementIndex, parents);
                _logger.TraceObject($"{Identifier} created validation context ", validationContext);

                if (!TryGetValueToValidate(objectToValidate, out var enumerator))
                {
                    _logger.Debug($"{Identifier} could not get values to validate. Skipping");
                    return errors.ToArray();
                }

                var values = enumerator.ToArray();

                for (int i = 0; i < values.Length; i++)
                {
                    var value = values[i];
                    validationContext.Value = value;
                    ModifyInfo(validationContext.Info, value, i);

                    _logger.Debug($"{Identifier} starting validation for value {i+1} <{value}> on <{objectToValidate}>");
                    var validators = await GetEnabledValidatorsFor(validationContext).ConfigureAwait(false);

                    if (validators.HasValue())
                    {
                        _logger.Debug($"{Identifier} using {validators.Length} validators for value {i + 1} <{value}> on <{objectToValidate}>");

                        for (int j = 0; j < validators.Length; j++)
                        {
                            var validator = validators[j];

                            try
                            {
                                if (!(await validator.Validator(validationContext).ConfigureAwait(false)))
                                {
                                    _logger.Debug($"{Identifier}: Value {i + 1} <{value}> on <{objectToValidate}> is not valid according to validator {j + 1}. Generating error");
                                    try
                                    {
                                        var error = await validator.ErrorContructor(validationContext).ConfigureAwait(false);

                                        if (error == null) throw new InvalidOperationException($"{Identifier}: Validator {j + 1} was told to generate an error for value {i + 1} <{value}> on <{objectToValidate}> but returned null");

                                        var validationError = new ValidationError<TError>()
                                        {
                                            Source = objectToValidate,
                                            Value = validationContext.Value,
                                            ElementIndex = elementIndex,
                                            Message = error,
                                            Parents = parents,
                                            DisplayName = validationContext.GetFullDisplayNameDynamically(false),
                                            FullDisplayName = validationContext.GetFullDisplayNameDynamically(true)
                                        };
                                        errors.Add(validationError);
                                    }
                                    catch(Exception ex)
                                    {
                                        _logger.Log($"{Identifier}: Error occured while executing error contructor delegate {j + 1} for value {i + 1} <{value}> on <{objectToValidate}>", ex);
                                        throw;
                                    }

                                    // Exit early when exit on invalid is enabled
                                    if (_settings.HasFlag(TargetExecutionOptions.ExitOnInvalid))
                                    {
                                        _logger.Debug($"{Identifier}: Exit on invalid enabled. Returning");
                                        break;
                                    }
                                }
                                else
                                {
                                    _logger.Debug($"{Identifier}: Value {i + 1} <{value}> on <{objectToValidate}> is valid according to validator {j + 1}.");
                                }
                            }
                            catch(Exception ex)
                            {
                                _logger.Log($"{Identifier}: Error occured while executing validator delegate {j + 1} for value {i + 1} <{value}> on <{objectToValidate}>", ex);
                                throw;
                            }

                            // Reset result between each rule execution
                            validationContext.ValidatorResult = null;
                        }
                    }
                    else
                    {
                        _logger.Debug($"{Identifier}: No validators enabled for value {i + 1} <{value}> on <{objectToValidate}>");
                    }
                }

                return errors.ToArray();
            }
        }

        // Virtuals
        /// <summary>
        /// Optional method for modifying <paramref name="info"/> using the current value that's being validated.
        /// </summary>
        /// <param name="info">Info object to modifiy</param>
        /// <param name="value">The current value that's being validated</param>
        /// <param name="index">Index of <paramref name="value"/> in the source collection</param>
        protected virtual void ModifyInfo(TInfo info, TValue value, int index)
        {

        }

        // Abstractions
        /// <summary>
        /// Tries to get the values to validate. Returns false when the value can't be returned (in case off null references).
        /// </summary>
        /// <param name="objectToValidate">Object to get the value from</param>
        /// <param name="values">Enumerator with the values to validate</param>
        /// <returns>Whether or not <paramref name="values"/> was set</returns>
        protected abstract bool TryGetValueToValidate(TEntity objectToValidate, out IEnumerable<TValue> values);
    }
}
