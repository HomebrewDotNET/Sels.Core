using Microsoft.Extensions.Logging;
using Sels.Core.Contracts.Validation;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.ObjectValidationFramework.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sels.ObjectValidationFramework.Validators
{
    /// <summary>
    /// Validator that delegates validation to <see cref="ValidationProfile{TError}"/>'s.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity to validate</typeparam>
    /// <typeparam name="TError">Type of validation error to return</typeparam>
    /// <typeparam name="TContext">Type of context that can be used to modify the behaviour of the validator</typeparam>
    internal class DynamicValidator<TEntity, TError, TContext> : IValidator<TEntity, TError, TContext>, IAsyncValidator<TEntity, TError, TContext>
    {
        // Fields
        private readonly IEnumerable<ILogger> _loggers;
        private readonly ValidationProfile<TError>[] _profiles;
        private readonly bool _contextIsRequired;

        /// <summary>
        /// Validator that delegates validation to <see cref="ValidationProfile{TError}"/>'s.
        /// </summary>
        /// <param name="profile">Profile to delegate validation to</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="loggers">Optional loggers for logging</param>
        public DynamicValidator(ValidationProfile<TError> profile, bool contextIsRequired = false, IEnumerable<ILogger> loggers = null) : this(contextIsRequired, loggers, profile.ValidateArgument(nameof(profile)))
        {

        }

        /// <summary>
        /// Validator that delegates validation to <see cref="ValidationProfile{TError}"/>'s.
        /// </summary>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="loggers">Optional loggers for logging</param>
        /// <param name="profiles">Profiles to delegate validation to</param>
        public DynamicValidator(bool contextIsRequired = false, IEnumerable < ILogger> loggers = null, params ValidationProfile<TError>[] profiles)
        {
            _profiles = profiles.ValidateArgumentNotNullOrEmpty(nameof(profiles));
            _loggers = loggers;
            _contextIsRequired = contextIsRequired;
        }

        /// <inheritdoc/>
        public TError[] Validate(TEntity entity, TContext context = default)
        {
            using (_loggers.TraceMethod(this))
            {
                entity.ValidateArgument(nameof(entity));
                if (_contextIsRequired) context.ValidateArgument(nameof(context));

                _loggers.Log($"Validating <{entity}> using {_profiles.Length} validation profiles");

                return _profiles.SelectMany(x => x.Validate(entity, context)).ToArray();
            }
        }
        /// <inheritdoc/>
        public TError[] Validate(IEnumerable<TEntity> entities, TContext context = default)
        {
            using (_loggers.TraceMethod(this))
            {
                entities.ValidateArgument(nameof(entities));
                if (_contextIsRequired) context.ValidateArgument(nameof(context));

                _loggers.Log($"Validating {entities.GetCount()} entities using {_profiles.Length} validation profiles");

                return _profiles.SelectMany(x => x.Validate(entities, context)).ToArray();
            }
        }
        /// <inheritdoc/>
        public async Task<TError[]> ValidateAsync(TEntity entity, TContext context = default)
        {
            using (_loggers.TraceMethod(this))
            {
                entity.ValidateArgument(nameof(entity));
                if (_contextIsRequired) context.ValidateArgument(nameof(context));

                _loggers.Log($"Validating <{entity}> using {_profiles.Length} validation profiles");
                var errors = new List<TError>();

                foreach(var profile in _profiles)
                {
                    errors.AddRange(await profile.ValidateAsync(entity, context));
                }

                return errors.ToArray();
            }
        }
        /// <inheritdoc/>
        public async Task<TError[]> ValidateAsync(IEnumerable<TEntity> entities, TContext context = default)
        {
            using (_loggers.TraceMethod(this))
            {
                entities.ValidateArgument(nameof(entities));
                if (_contextIsRequired) context.ValidateArgument(nameof(context));

                _loggers.Log($"Validating {entities.GetCount()} entities using {_profiles.Length} validation profiles");

                var errors = new List<TError>();

                foreach (var profile in _profiles)
                {
                    errors.AddRange(await profile.ValidateAsync(entities, context));
                }

                return errors.ToArray();
            }
        }
    }
}
