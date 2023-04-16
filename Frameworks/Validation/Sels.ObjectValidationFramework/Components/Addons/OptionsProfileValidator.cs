using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Extensions.Reflection;
using Sels.ObjectValidationFramework.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.ObjectValidationFramework.Addons
{
    /// <summary>
    /// Validates options of type <typeparamref name="TOption"/> using validation profiles.
    /// </summary>
    /// <typeparam name="TOption">Type of the options to validate</typeparam>
    internal class OptionsProfileValidator<TOption> : IValidateOptions<TOption> where TOption : class
    {
        // Fields
        private readonly Func<ValidationError<string>, string> _projector;
        private readonly ValidationProfile<string>[] _profiles;
        private readonly object _context;
        private readonly ProfileExecutionOptions _executionOptions;
        private readonly string[] _targets;
        private readonly ILogger _logger;

        /// <inheritdoc cref="OptionsProfileValidator{TOptions}"/>
        /// <param name="profiles">The validation profiles used to validate the options instance</param>
        /// <param name="context">Optional context for the profiles</param>
        /// <param name="executionOptions">The options to use when calling the validation profiles</param>
        /// <param name="projector">Optional projector to modify the error messages returned from the profiles. 
        /// The default projector prefixes the error message like {Property}: {ErrorMessage} if a property is available for the error message</param>
        /// <param name="logger">Optional logger for tracing</param>
        /// <param name="targets">The names of the options instances the current validator can target. If set to null or empty all options will be validated</param>
        public OptionsProfileValidator(IEnumerable<ValidationProfile<string>> profiles, object context, ProfileExecutionOptions executionOptions, Func<ValidationError<string>, string> projector, ILogger<OptionsProfileValidator<TOption>> logger, params string[] targets)
        {
            _profiles = profiles.ValidateArgumentNotNullOrEmpty(nameof(profiles)).ToArray();
            _context = context;
            _executionOptions = executionOptions;
            _projector = projector ?? Project;
            _targets = targets;
            _logger = logger;
        }

        /// <inheritdoc/>
        public ValidateOptionsResult Validate(string name, TOption options)
        {
            using (_logger.TraceMethod(this))
            {
                // Skip if we have targets and the name of the options is not in the target array
                if (_targets.HasValue() && !_targets.Contains(name))
                {
                    _logger.Log($"Option <{typeof(TOption).GetDisplayName(false)}> with name <{name}> is not in the target list of <{_targets.JoinString(';')}>. Skipping validation");
                    return ValidateOptionsResult.Skip;
                }

                _logger.Log($"Validating option <{typeof(TOption).GetDisplayName(false)}> with name <{name}> using <{_profiles.Length}> validation profiles");
                // Start validation
                var errors = new List<string>();
                foreach (var profile in _profiles)
                {
                    _logger.Debug($"Validating option <{typeof(TOption).GetDisplayName(false)}> with name <{name}> using profile <{profile}>");
                    var result = profile.Validate(options, _context, _executionOptions);
                    if (!result.IsValid)
                    {
                        _logger.Debug($"Option <{typeof(TOption).GetDisplayName(false)}> with name <{name}> is not valid according to profile <{profile}> and returned <{result.Errors.Length}> errors");
                        errors.AddRange(result.ChangeMessageTo(_projector).Messages);
                    }
                }

                if (errors.HasValue())
                {
                    _logger.Log($"Option <{typeof(TOption).GetDisplayName(false)}> with name <{name}> is not valid");
                    var errorBuilder = new StringBuilder();
                    errorBuilder.Append("Option <").Append(typeof(TOption).GetDisplayName(false)).Append("> with name <").Append(name).Append("> is not valid: ");
                    foreach (var error in errors)
                    {
                        errorBuilder.AppendLine().Append(error);
                    }
                    return ValidateOptionsResult.Fail(errorBuilder.ToString());
                }
                _logger.Log($"Option <{typeof(TOption).GetDisplayName(false)}> with name <{name}> is valid");
                return ValidateOptionsResult.Success;
            }
        }

        private string Project(ValidationError<string> error) => $"{(error.Property != null ? $"{error.Property.Name}: " : string.Empty)}{error.Message}";
    }
}
