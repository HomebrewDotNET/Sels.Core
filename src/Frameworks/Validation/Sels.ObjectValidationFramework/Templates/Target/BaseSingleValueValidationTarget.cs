using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Logging;
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
    /// Template for creating a validation rule that validates a single value.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
    /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
    /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal abstract class BaseSingleValueValidationTarget<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> : BaseContextValidationTarget<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> where TTargetContext : TBaseContext
    {
        /// <inheritdoc cref="BaseSingleValueValidationTarget{TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue}"/>.
        /// <param name="validator"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._validator"/></param>
        /// <param name="settings"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._settings"/></param>
        /// <param name="globalConditions"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._globalConditions"/></param>
        /// <param name="logger"><inheritdoc cref="BaseValidationTarget{TEntity, TContext, TError}._logger"/></param>
        internal BaseSingleValueValidationTarget(EntityValidator<TEntity, TBaseContext, TError> validator, TargetExecutionOptions settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, TBaseContext>>> globalConditions = null, ILogger logger = null) : base(validator, settings, globalConditions, logger)
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


                if (!TryGetValueToValidate(objectToValidate, out var value))
                {
                    _logger.Debug($"{Identifier} could not get values to validate. Skipping");
                    return errors.ToArray();
                }

                validationContext.Value = value;

                _logger.Debug($"{Identifier} starting validation for value <{value}> on <{objectToValidate}>");
                var validators = await GetEnabledValidatorsFor(validationContext).ConfigureAwait(false);

                if (validators.HasValue())
                {
                    _logger.Debug($"{Identifier}: Using {validators.Length} validators for value <{value}> on <{objectToValidate}>");

                    for(int i = 0; i < validators.Length; i++)
                    {
                        var validator = validators[i];

                        try
                        {
                            if (!( await validator.Validator(validationContext).ConfigureAwait(false)))
                            {
                                _logger.Debug($"{Identifier}: Value <{value}> on <{objectToValidate}> is not valid according to validator {i + 1}. Generating error");
                                try
                                {
                                    var error = await validator.ErrorContructor(validationContext).ConfigureAwait(false);

                                    if (error == null) throw new InvalidOperationException($"{Identifier}: Validator {i + 1} was told to generate an error for value <{value}> on <{objectToValidate}> but returned null");

                                    var validationError = new ValidationError<TError>()
                                    {
                                        Source = objectToValidate,
                                        Value = validationContext.Value,
                                        ElementIndex = elementIndex,
                                        Message = error,
                                        Parents = parents,
                                        DisplayName = GetDisplayNameFor(validationContext, false),
                                        FullDisplayName = GetDisplayNameFor(validationContext, true)
                                    };
                                    errors.Add(validationError);
                                }
                                catch(Exception ex)
                                {
                                    _logger.Log($"{Identifier}: Error occured while executing error contructor delegate {i + 1} for value <{value}> on <{objectToValidate}>", ex);
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
                                _logger.Debug($"{Identifier}: Value <{value}> on <{objectToValidate}> is valid according to validator {i + 1}.");
                            }
                        }
                        catch(Exception ex)
                        {
                            _logger.Log($"{Identifier}: Error occured while executing validator delegate {i + 1} for value <{value}> on <{objectToValidate}>", ex);
                            throw;
                        }                       
                    }
                }
                else
                {
                    _logger.Debug($"{Identifier}: No validators enabled for value <{value}> on <{objectToValidate}>");
                }

                return errors.ToArray();
            }
        }

        // Abstractions
        /// <summary>
        /// Tries to get the value to validate. Returns false when the value can't be returned (in case of null references).
        /// </summary>
        /// <param name="objectToValidate">Object to get the value from</param>
        /// <param name="value">The value to validate</param>
        /// <returns>Whether or not <paramref name="value"/> was set</returns>
        protected abstract bool TryGetValueToValidate(TEntity objectToValidate, out TValue value);
    }
}
