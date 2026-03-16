using Microsoft.Extensions.Logging;
using Sels.Core.Validation;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging;
using Sels.ObjectValidationFramework.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sels.ObjectValidationFramework.Addons
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
        private readonly ILogger _logger;
        private readonly ValidationProfile<TError>[] _profiles;
        private readonly bool _contextIsRequired;

        /// <summary>
        /// Validator that delegates validation to <see cref="ValidationProfile{TError}"/>'s.
        /// </summary>
        /// <param name="profile">Profile to delegate validation to</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="logger">Optional loggers for tracing</param>
        public DynamicValidator(ValidationProfile<TError> profile, bool contextIsRequired = false, ILogger<DynamicValidator<TEntity, TError, TContext>> logger = null) : this(contextIsRequired, logger, profile.ValidateArgument(nameof(profile)))
        {

        }

        /// <summary>
        /// Validator that delegates validation to <see cref="ValidationProfile{TError}"/>'s.
        /// </summary>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="logger">Optional loggers for tracing</param>
        /// <param name="profiles">Profiles to delegate validation to</param>
        public DynamicValidator(bool contextIsRequired = false, ILogger<DynamicValidator<TEntity, TError, TContext>> logger = null, params ValidationProfile<TError>[] profiles)
        {
            _profiles = profiles.ValidateArgumentNotNullOrEmpty(nameof(profiles));
            _logger = logger;
            _contextIsRequired = contextIsRequired;
        }

        /// <inheritdoc/>
        public TError[] Validate(TEntity entity, TContext context = default)
        {
            using (_logger.TraceMethod(this))
            {
                entity.ValidateArgument(nameof(entity));
                if (_contextIsRequired) context.ValidateArgument(nameof(context));

                _logger.Log($"Validating <{entity}> using {_profiles.Length} validation profiles");

                return _profiles.SelectMany(x => x.Validate(entity, context).Errors.Select(e => e.Message)).ToArray();
            }
        }
        /// <inheritdoc/>
        public TError[] Validate(IEnumerable<TEntity> entities, TContext context = default)
        {
            using (_logger.TraceMethod(this))
            {
                entities.ValidateArgument(nameof(entities));
                if (_contextIsRequired) context.ValidateArgument(nameof(context));

                _logger.Log($"Validating {entities.GetCount()} entities using {_profiles.Length} validation profiles");

                return _profiles.SelectMany(x => x.Validate(entities, context).Errors.Select(e => e.Message)).ToArray();
            }
        }
        /// <inheritdoc/>
        public async Task<TError[]> ValidateAsync(TEntity entity, TContext context = default)
        {
            using (_logger.TraceMethod(this))
            {
                entity.ValidateArgument(nameof(entity));
                if (_contextIsRequired) context.ValidateArgument(nameof(context));

                _logger.Log($"Validating <{entity}> using {_profiles.Length} validation profiles");
                var errors = new List<TError>();

                foreach (var profile in _profiles)
                {
                    var result = await profile.ValidateAsync(entity, context).ConfigureAwait(false);
                    errors.AddRange(result.Errors.Select(x => x.Message));
                }

                return errors.ToArray();
            }
        }
        /// <inheritdoc/>
        public async Task<TError[]> ValidateAsync(IEnumerable<TEntity> entities, TContext context = default)
        {
            using (_logger.TraceMethod(this))
            {
                entities.ValidateArgument(nameof(entities));
                if (_contextIsRequired) context.ValidateArgument(nameof(context));

                _logger.Log($"Validating {entities.GetCount()} entities using {_profiles.Length} validation profiles");

                var errors = new List<TError>();

                foreach (var profile in _profiles)
                {
                    var result = await profile.ValidateAsync(entities, context).ConfigureAwait(false);
                    errors.AddRange(result.Errors.Select(x => x.Message));
                }

                return errors.ToArray();
            }
        }
    }

    /// <summary>
    /// Validator that delegates validation to <see cref="ValidationProfile{TError}"/>'s.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity to validate</typeparam>
    /// <typeparam name="TError">Type of the validation errors returned by the profiles</typeparam>
    /// <typeparam name="TNewError">Type of the error message returned by the current validator</typeparam>
    /// <typeparam name="TContext">Type of context that can be used to modify the behaviour of the validator</typeparam>
    internal class DynamicValidator<TEntity, TError, TNewError, TContext> : IValidator<TEntity, TNewError, TContext>, IAsyncValidator<TEntity, TNewError, TContext>
    {
        // Fields
        private readonly ILogger _logger;
        private readonly ValidationProfile<TError>[] _profiles;
        private readonly bool _contextIsRequired;
        private readonly Func<ValidationError<TError>, TNewError> _projector;

        /// <summary>
        /// Validator that delegates validation to <see cref="ValidationProfile{TError}"/>'s.
        /// </summary>
        /// <param name="projector">Delegate that converts the errors returned by the profiles to the intended error type</param>
        /// <param name="profile">Profile to delegate validation to</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="logger">Optional loggers for tracing</param>
        public DynamicValidator(Func<ValidationError<TError>, TNewError> projector, ValidationProfile<TError> profile, bool contextIsRequired = false, ILogger<DynamicValidator<TEntity, TError, TNewError, TContext>> logger = null) : this(projector, contextIsRequired, logger, profile.ValidateArgument(nameof(profile)))
        {

        }

        /// <summary>
        /// Validator that delegates validation to <see cref="ValidationProfile{TError}"/>'s.
        /// </summary>
        /// <param name="projector">Delegate that converts the errors returned by the profiles to the intended error type</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="logger">Optional loggers for tracing</param>
        /// <param name="profiles">Profiles to delegate validation to</param>
        public DynamicValidator(Func<ValidationError<TError>, TNewError> projector, bool contextIsRequired = false, ILogger<DynamicValidator<TEntity, TError, TNewError, TContext>> logger = null, params ValidationProfile<TError>[] profiles)
        {
            _projector = projector.ValidateArgument(nameof(projector));
            _profiles = profiles.ValidateArgumentNotNullOrEmpty(nameof(profiles));
            _logger = logger;
            _contextIsRequired = contextIsRequired;
        }

        /// <inheritdoc/>
        public TNewError[] Validate(TEntity entity, TContext context = default)
        {
            using (_logger.TraceMethod(this))
            {
                entity.ValidateArgument(nameof(entity));
                if (_contextIsRequired) context.ValidateArgument(nameof(context));

                _logger.Log($"Validating <{entity}> using {_profiles.Length} validation profiles");

                return _profiles.SelectMany(x => x.Validate(entity, context).ChangeMessageTo(_projector).Errors.Select(e => e.Message)).ToArray();
            }
        }
        /// <inheritdoc/>
        public TNewError[] Validate(IEnumerable<TEntity> entities, TContext context = default)
        {
            using (_logger.TraceMethod(this))
            {
                entities.ValidateArgument(nameof(entities));
                if (_contextIsRequired) context.ValidateArgument(nameof(context));

                _logger.Log($"Validating {entities.GetCount()} entities using {_profiles.Length} validation profiles");

                return _profiles.SelectMany(x => x.Validate(entities, context).ChangeMessageTo(_projector).Errors.Select(e => e.Message)).ToArray();
            }
        }
        /// <inheritdoc/>
        public async Task<TNewError[]> ValidateAsync(TEntity entity, TContext context = default)
        {
            using (_logger.TraceMethod(this))
            {
                entity.ValidateArgument(nameof(entity));
                if (_contextIsRequired) context.ValidateArgument(nameof(context));

                _logger.Log($"Validating <{entity}> using {_profiles.Length} validation profiles");
                var errors = new List<TNewError>();

                foreach (var profile in _profiles)
                {
                    var result = await profile.ValidateAsync(entity, context).ConfigureAwait(false);
                    errors.AddRange(result.ChangeMessageTo(_projector).Errors.Select(x => x.Message));
                }

                return errors.ToArray();
            }
        }
        /// <inheritdoc/>
        public async Task<TNewError[]> ValidateAsync(IEnumerable<TEntity> entities, TContext context = default)
        {
            using (_logger.TraceMethod(this))
            {
                entities.ValidateArgument(nameof(entities));
                if (_contextIsRequired) context.ValidateArgument(nameof(context));

                _logger.Log($"Validating {entities.GetCount()} entities using {_profiles.Length} validation profiles");

                var errors = new List<TNewError>();

                foreach (var profile in _profiles)
                {
                    var result = await profile.ValidateAsync(entities, context).ConfigureAwait(false);
                    errors.AddRange(result.ChangeMessageTo(_projector).Errors.Select(x => x.Message));
                }

                return errors.ToArray();
            }
        }
    }
}
