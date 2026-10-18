using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging;
using Sels.ObjectValidationFramework.Addons;
using Sels.ObjectValidationFramework.Profile;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sels.ObjectValidationFramework.Components.Addons
{
    /// <summary>
    /// Validator that delegates validation to <see cref="ValidationProfile{TError}"/>'s.
    /// </summary>
    /// <typeparam name="TEntity">Type of the entity to validate</typeparam>
    /// <typeparam name="TError">Type of the validation error</typeparam>
    /// <typeparam name="TContext">Type of the validation context</typeparam>
    public class MergedValidationProfile<TEntity, TError, TContext> : IValidationProfile<TEntity, TError, TContext>
    {
        // Fields
        private readonly ILogger _logger;
        private readonly ValidationProfile<TError> _merged;
        private readonly ValidationProfile<TError>[] _profiles;
        private readonly bool _contextIsRequired;

        /// <summary>
        /// Validator that delegates validation to <see cref="ValidationProfile{TError}"/>'s.
        /// </summary>
        /// <param name="profile">Profile to delegate validation to</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="logger">Optional loggers for tracing</param>
        public MergedValidationProfile(ValidationProfile<TError> profile, bool contextIsRequired = false, ILogger<MergedValidationProfile<TEntity, TError, TContext>> logger = null) : this(contextIsRequired, logger, profile.ValidateArgument(nameof(profile)))
        {

        }

        /// <summary>
        /// Validator that delegates validation to <see cref="ValidationProfile{TError}"/>'s.
        /// </summary>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="logger">Optional loggers for tracing</param>
        /// <param name="profiles">Profiles to delegate validation to</param>
        public MergedValidationProfile(bool contextIsRequired = false, ILogger<MergedValidationProfile<TEntity, TError, TContext>> logger = null, params ValidationProfile<TError>[] profiles)
        {
            _profiles = profiles.ValidateArgumentNotNullOrEmpty(nameof(profiles));
            _logger = logger;
            _contextIsRequired = contextIsRequired;
            if (_profiles.Length > 1)
            {
                _merged = new ValidationProfile<TError>();
                _profiles.Execute(x => _merged.ImportFrom(x, ImportOptions.All));
            }
            else
            {
                _merged = _profiles[0];
            }
        }
        /// <inheritdoc/>
        public ValidationResult<TEntity, TError> Validate(TEntity objectToValidate, TContext context = default, ProfileExecutionOptions options = ProfileExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                objectToValidate.ValidateArgument(nameof(objectToValidate));
                if (_contextIsRequired) context.ValidateArgument(nameof(context));

                _logger.Log($"Validating <{objectToValidate}> using {_profiles.Length} validation profiles");

                return _merged.Validate(objectToValidate, context, options);
            }
        }
        /// <inheritdoc/>
        public async Task<ValidationResult<TEntity, TError>> ValidateAsync(TEntity objectToValidate, TContext context = default, ProfileExecutionOptions options = ProfileExecutionOptions.None)
        {
            using (_logger.TraceMethod(this))
            {
                objectToValidate.ValidateArgument(nameof(objectToValidate));
                if (_contextIsRequired) context.ValidateArgument(nameof(context));

                _logger.Log($"Validating <{objectToValidate}> using {_profiles.Length} validation profiles");

                return await _merged.ValidateAsync(objectToValidate, context, options).ConfigureAwait(false);
            }
        }
    }
}
