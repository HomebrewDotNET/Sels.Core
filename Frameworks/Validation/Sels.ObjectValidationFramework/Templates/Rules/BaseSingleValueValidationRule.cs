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
using System.Text;

namespace Sels.ObjectValidationFramework.Templates.Rules
{
    /// <summary>
    /// Template for creating a validation rule that validates a single value.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
    /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal abstract class BaseSingleValueValidationRule<TEntity, TError, TInfo, TContext, TValue> : BaseContextValidationRule<TEntity, TError, TInfo, TContext, TValue>
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="validator">Validator to delegate <see cref="IValidationConfigurator{TEntity, TError}"/> calls to </param>
        /// <param name="globalConditions">Global conditions that all need to pass before any validation rules are allowed to run</param>
        /// <param name="settings">Extra settings for the rule</param>
        /// <param name="loggers">Option loggers for logging</param>
        internal BaseSingleValueValidationRule(EntityValidator<TEntity, TError> validator, RuleSettings settings, IEnumerable<Predicate<IValidationRuleContext<TEntity, object>>> globalConditions = null, IEnumerable<ILogger> loggers = null) : base(validator, settings, globalConditions, loggers)
        {
        }

        /// <inheritdoc/>
        internal override TError[] Validate(TEntity objectToValidate, object context, int? elementIndex = null, Parent[] parents = null)
        {
            using (_loggers.TraceMethod(this))
            {
                objectToValidate.ValidateArgument(nameof(objectToValidate));
                var errors = new List<TError>();

                _loggers.TraceObject($"Starting validation for ", objectToValidate);

                _loggers.Debug($"Creating validation context");
                var validationContext = CreateContext(objectToValidate, context, elementIndex, parents);
                _loggers.TraceObject($"Created validation context ", validationContext);

                if(!TryGetValueToValidate(objectToValidate, out var value))
                {
                    _loggers.Debug($"Could not get value to validate. Skipping");
                    return errors.ToArray();
                }

                validationContext.Value = value;

                _loggers.Debug($"Starting validation for value <{value}> on <{objectToValidate}>");
                var validators = GetEnabledValidatorsFor(validationContext);

                if (validators.HasValue())
                {
                    _loggers.Debug($"Using {validators.Length} validators for value <{value}> on <{objectToValidate}>");

                    for(int i = 0; i < validators.Length; i++)
                    {
                        var validator = validators[i];

                        try
                        {
                            if (!validator.Validator(validationContext))
                            {
                                _loggers.Debug($"Value <{value}> on <{objectToValidate}> is not valid according to validator {i + 1}. Generating error");
                                try
                                {
                                    var error = validator.ErrorContructor(validationContext);

                                    if (error == null)
                                    {
                                        _loggers.Warning($"Validator {i + 1} was told to generate an error for value <{value}> on <{objectToValidate}> but returned null");
                                        continue;
                                    }

                                    errors.Add(error);
                                }
                                catch(Exception ex)
                                {
                                    _loggers.LogException(LogLevel.Warning, $"Error occured while executing error contructor delegate {i + 1} for value <{value}> on <{objectToValidate}>", ex);
                                }
                            }
                            else
                            {
                                _loggers.Debug($"Value <{value}> on <{objectToValidate}> is valid according to validator {i + 1}.");
                            }
                        }
                        catch(Exception ex)
                        {
                            _loggers.LogException(LogLevel.Warning, $"Error occured while executing validator delegate {i + 1} for value <{value}> on <{objectToValidate}>", ex);
                            throw;
                        }                       
                    }
                }
                else
                {
                    _loggers.Debug($"No validators enabled for value <{value}> on <{objectToValidate}>");
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
