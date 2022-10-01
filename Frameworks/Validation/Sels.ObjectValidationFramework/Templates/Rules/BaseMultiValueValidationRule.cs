using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.ObjectValidationFramework.Components.Validators;
using Sels.ObjectValidationFramework.Contracts.Rules;
using Sels.ObjectValidationFramework.Contracts.Validators;
using Sels.ObjectValidationFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.ObjectValidationFramework.Templates.Rules
{
    /// <summary>
    /// Template for creating a validation rule that validates a multiple values.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
    /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal abstract class BaseMultiValueValidationRule<TEntity, TError, TInfo, TContext, TValue> : BaseContextValidationRule<TEntity, TError, TInfo, TContext, TValue>
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="validator">Validator to delegate <see cref="IValidationConfigurator{TEntity, TError}"/> calls to </param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <param name="globalConditions">Global conditions that all need to pass before any validation rules are allowed to run</param>
        /// <param name="loggers">Option loggers for logging</param>
        internal BaseMultiValueValidationRule(EntityValidator<TEntity, TError> validator, RuleSettings settings, IEnumerable<AsyncPredicate<IValidationRuleContext<TEntity, object>>> globalConditions = null, IEnumerable<ILogger> loggers = null) : base(validator, settings, globalConditions, loggers)
        {
        }

        /// <inheritdoc/>
        internal override async Task<TError[]> Validate(TEntity objectToValidate, object context, int? elementIndex = null, Parent[] parents = null)
        {
            using (_loggers.TraceMethod(this))
            {
                objectToValidate.ValidateArgument(nameof(objectToValidate));
                var errors = new List<TError>();

                _loggers.TraceObject($"Starting validation for ", objectToValidate);

                _loggers.Debug($"Creating validation context");
                var validationContext = CreateContext(objectToValidate, context, elementIndex, parents);
                _loggers.TraceObject($"Created validation context ", validationContext);

                if (!TryGetValueToValidate(objectToValidate, out var enumerator))
                {
                    _loggers.Debug($"Could not get values to validate. Skipping");
                    return errors.ToArray();
                }

                var values = enumerator.ToArray();

                for (int i = 0; i < values.Length; i++)
                {
                    var value = values[i];
                    validationContext.Value = value;
                    ModifyInfo(validationContext.Info, value, i);

                    _loggers.Debug($"Starting validation for value {i+1} <{value}> on <{objectToValidate}>");
                    var validators = await GetEnabledValidatorsFor(validationContext);

                    if (validators.HasValue())
                    {
                        _loggers.Debug($"Using {validators.Length} validators for value {i + 1} <{value}> on <{objectToValidate}>");

                        for (int j = 0; j < validators.Length; j++)
                        {
                            var validator = validators[j];

                            try
                            {
                                if (!(await validator.Validator(validationContext)))
                                {
                                    _loggers.Debug($"Value {i + 1} <{value}> on <{objectToValidate}> is not valid according to validator {j + 1}. Generating error");
                                    try
                                    {
                                        var error = await validator.ErrorContructor(validationContext);

                                        if (error == null)
                                        {
                                            _loggers.Warning($"Validator {j + 1} was told to generate an error for value {i + 1} <{value}> on <{objectToValidate}> but returned null");
                                            continue;
                                        }

                                        errors.Add(error);
                                    }
                                    catch(Exception ex)
                                    {
                                        _loggers.LogException(LogLevel.Warning, $"Error occured while executing error contructor delegate {j + 1} for value {i + 1} <{value}> on <{objectToValidate}>", ex);
                                        throw;
                                    }
                                }
                                else
                                {
                                    _loggers.Debug($"Value {i + 1} <{value}> on <{objectToValidate}> is valid according to validator {j + 1}.");
                                }
                            }
                            catch(Exception ex)
                            {
                                _loggers.LogException(LogLevel.Warning, $"Error occured while executing validator delegate {j + 1} for value {i + 1} <{value}> on <{objectToValidate}>", ex);
                                throw;
                            }
                            
                        }
                    }
                    else
                    {
                        _loggers.Debug($"No validators enabled for value {i + 1} <{value}> on <{objectToValidate}>");
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
        /// Tries to get the values to validate. Returns false when the value can't returned (in case off null references).
        /// </summary>
        /// <param name="objectToValidate">Object to get the value from</param>
        /// <param name="values">Enumerator with the values to validate</param>
        /// <returns>Whether or not <paramref name="values"/> was set</returns>
        protected abstract bool TryGetValueToValidate(TEntity objectToValidate, out IEnumerable<TValue> values);
    }
}
