using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sels.Core.Validation;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.ObjectValidationFramework.Addons;
using Sels.ObjectValidationFramework.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sels.Core.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for registering services into a <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ApplicationRegistrations
    {
        #region Profile
        /// <summary>
        /// Registers validation profile <typeparamref name="TProfile"/> into the service collection.
        /// </summary>
        /// <typeparam name="TProfile">Type of profile to register</typeparam>
        /// <typeparam name="TError">Type of validation error that the profile returns</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddValidationProfile<TProfile, TError>(this IServiceCollection services, ServiceLifetime scope = ServiceLifetime.Scoped) where TProfile : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            services.Register<ValidationProfile<TError>, TProfile>(scope);
            services.Register<TProfile>(scope);

            return services;
        }

        /// <summary>
        /// Registers validation profile <typeparamref name="TProfile"/> into the service collection.
        /// </summary>
        /// <typeparam name="TProfile">Type of profile to register</typeparam>
        /// <typeparam name="TError">Type of validation error that the profile returns</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="profileFactory">Factory that creates instances of <typeparamref name="TProfile"/></param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddValidationProfile<TProfile, TError>(this IServiceCollection services, Func<IServiceProvider, TProfile> profileFactory, ServiceLifetime scope = ServiceLifetime.Scoped) where TProfile : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            services.Register<ValidationProfile<TError>, TProfile>(profileFactory, scope);
            services.Register(profileFactory, scope);

            return services;
        }
        #endregion

        #region Validator
        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <typeparam name="TContext">Type of context that can be used to modify the behaviour of the validator</typeparam>
        /// <typeparam name="TProfile">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddValidatorWithContext<TEntity, TError, TContext, TProfile>(this IServiceCollection services, bool contextIsRequired = false) where TProfile : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            services.Register<IValidator<TEntity, TError, TContext>, DynamicValidator<TEntity, TError, TContext>>(x =>
            {
                var profile = x.GetRequiredService<TProfile>();

                return new DynamicValidator<TEntity, TError, TContext>(profile, contextIsRequired, x.GetService<ILogger<DynamicValidator<TEntity, TError, TContext>>>());
            }, ServiceLifetime.Scoped);

            return services;
        }

        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <typeparam name="TProfile">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddValidator<TEntity, TError, TProfile>(this IServiceCollection services, bool contextIsRequired = false) where TProfile : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            return services.AddValidatorWithContext<TEntity, TError, object, TProfile>(contextIsRequired);
        }

        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <typeparam name="TContext">Type of context that can be used to modify the behaviour of the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="profileConstructor">Delegate that returns the validation profiles to include in the validator</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddValidatorWithContext<TEntity, TError, TContext>(this IServiceCollection services, Func<IServiceProvider, IEnumerable<ValidationProfile<TError>>> profileConstructor, bool contextIsRequired = false)
        {
            services.ValidateArgument(nameof(services));
            profileConstructor.ValidateArgument(nameof(profileConstructor));

            services.Register<IValidator<TEntity, TError, TContext>, DynamicValidator<TEntity, TError, TContext>>(x =>
            {
                var profiles = profileConstructor(x) ?? throw new InvalidOperationException($"{nameof(profileConstructor)} returned null");

                return new DynamicValidator<TEntity, TError, TContext>(contextIsRequired, x.GetService<ILogger<DynamicValidator<TEntity, TError, TContext>>>(), profiles.ToArray());
            }, ServiceLifetime.Scoped);

            return services;
        }

        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="profileConstructor">Delegate that returns the validation profiles to include in the validator</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddValidator<TEntity, TError>(this IServiceCollection services, Func<IServiceProvider, IEnumerable<ValidationProfile<TError>>> profileConstructor, bool contextIsRequired = false)
        {
            services.ValidateArgument(nameof(services));
            profileConstructor.ValidateArgument(nameof(profileConstructor));

            return services.AddValidatorWithContext<TEntity, TError, object>(profileConstructor, contextIsRequired);    
        }
        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error returned by the profiles</typeparam>
        /// <typeparam name="TNewError">Type of validation error returned by the validator</typeparam>
        /// <typeparam name="TContext">Type of context that can be used to modify the behaviour of the validator</typeparam>
        /// <typeparam name="TProfile">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="errorProjector">Delegate that converts the errors returned by the validation profiles to the intended error type</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddValidatorWithContext<TEntity, TError, TNewError, TContext, TProfile>(this IServiceCollection services, Func<ValidationError<TError>, TNewError> errorProjector, bool contextIsRequired = false) where TProfile : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));
            errorProjector.ValidateArgument(nameof(errorProjector));

            services.Register<IValidator<TEntity, TNewError, TContext>, DynamicValidator<TEntity, TError, TNewError, TContext>>(x =>
            {
                var profile = x.GetRequiredService<TProfile>();

                return new DynamicValidator<TEntity, TError, TNewError, TContext>(errorProjector, profile, contextIsRequired, x.GetService<ILogger<DynamicValidator<TEntity, TError, TNewError, TContext>>>());
            }, ServiceLifetime.Scoped);

            return services;
        }

        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error returned by the profiles</typeparam>
        /// <typeparam name="TNewError">Type of validation error returned by the validator</typeparam>
        /// <typeparam name="TProfile">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="errorProjector">Delegate that converts the errors returned by the validation profiles to the intended error type</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddValidator<TEntity, TError, TNewError, TProfile>(this IServiceCollection services, Func<ValidationError<TError>, TNewError> errorProjector, bool contextIsRequired = false) where TProfile : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));
            errorProjector.ValidateArgument(nameof(errorProjector));

            return services.AddValidatorWithContext<TEntity, TError, TNewError, object, TProfile>(errorProjector, contextIsRequired);
        }

        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error returned by the profiles</typeparam>
        /// <typeparam name="TNewError">Type of validation error returned by the validator</typeparam>
        /// <typeparam name="TContext">Type of context that can be used to modify the behaviour of the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="errorProjector">Delegate that converts the errors returned by the validation profiles to the intended error type</param>
        /// <param name="profileConstructor">Delegate that returns the validation profiles to include in the validator</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddValidatorWithContext<TEntity, TError, TNewError, TContext>(this IServiceCollection services, Func<ValidationError<TError>, TNewError> errorProjector, Func<IServiceProvider, IEnumerable<ValidationProfile<TError>>> profileConstructor, bool contextIsRequired = false)
        {
            services.ValidateArgument(nameof(services));
            errorProjector.ValidateArgument(nameof(errorProjector));
            profileConstructor.ValidateArgument(nameof(profileConstructor));

            services.Register<IValidator<TEntity, TNewError, TContext>, DynamicValidator<TEntity, TError, TNewError, TContext>>(x =>
            {
                var profiles = profileConstructor(x) ?? throw new InvalidOperationException($"{nameof(profileConstructor)} returned null");

                return new DynamicValidator<TEntity, TError, TNewError, TContext>(errorProjector, contextIsRequired, x.GetService<ILogger<DynamicValidator<TEntity, TError, TNewError, TContext>>>(), profiles.ToArray());
            }, ServiceLifetime.Scoped);

            return services;
        }

        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error returned by the profiles</typeparam>
        /// <typeparam name="TNewError">Type of validation error returned by the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="errorProjector">Delegate that converts the errors returned by the validation profiles to the intended error type</param>
        /// <param name="profileConstructor">Delegate that returns the validation profiles to include in the validator</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddValidator<TEntity, TError, TNewError>(this IServiceCollection services, Func<ValidationError<TError>, TNewError> errorProjector, Func<IServiceProvider, IEnumerable<ValidationProfile<TError>>> profileConstructor, bool contextIsRequired = false)
        {
            services.ValidateArgument(nameof(services));
            errorProjector.ValidateArgument(nameof(errorProjector));
            profileConstructor.ValidateArgument(nameof(profileConstructor));

            return AddValidatorWithContext<TEntity, TError, TNewError, object>(services, errorProjector, profileConstructor, contextIsRequired);
        }
        #endregion

        #region AsyncValidator
        /// <summary>
        /// Registers a new <see cref="IAsyncValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <typeparam name="TContext">Type of context that can be used to modify the behaviour of the validator</typeparam>
        /// <typeparam name="TProfile">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddAsyncValidatorWithContext<TEntity, TError, TContext, TProfile>(this IServiceCollection services, bool contextIsRequired = false) where TProfile : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            services.Register<IAsyncValidator<TEntity, TError, TContext>, DynamicValidator<TEntity, TError, TContext>>(x =>
            {
                var profile = x.GetRequiredService<TProfile>();

                return new DynamicValidator<TEntity, TError, TContext>(profile, contextIsRequired, x.GetService<ILogger<DynamicValidator<TEntity, TError, TContext>>>());
            }, ServiceLifetime.Scoped);

            return services;
        }

        /// <summary>
        /// Registers a new <see cref="IAsyncValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <typeparam name="TProfile">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddAsyncValidator<TEntity, TError, TProfile>(this IServiceCollection services, bool contextIsRequired = false) where TProfile : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            return services.AddAsyncValidatorWithContext<TEntity, TError, object, TProfile>(contextIsRequired);
        }

        /// <summary>
        /// Registers a new <see cref="IAsyncValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <typeparam name="TContext">Type of context that can be used to modify the behaviour of the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="profileConstructor">Delegate that returns the validation profiles to include in the validator</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddAsyncValidatorWithContext<TEntity, TError, TContext>(this IServiceCollection services, Func<IServiceProvider, IEnumerable<ValidationProfile<TError>>> profileConstructor, bool contextIsRequired = false)
        {
            services.ValidateArgument(nameof(services));
            profileConstructor.ValidateArgument(nameof(profileConstructor));

            services.Register<IAsyncValidator<TEntity, TError, TContext>, DynamicValidator<TEntity, TError, TContext>>(x =>
            {
                var profiles = profileConstructor(x) ?? throw new InvalidOperationException($"{nameof(profileConstructor)} returned null");

                return new DynamicValidator<TEntity, TError, TContext>(contextIsRequired, x.GetService<ILogger<DynamicValidator<TEntity, TError, TContext>>>(), profiles.ToArray());
            }, ServiceLifetime.Scoped);

            return services;
        }

        /// <summary>
        /// Registers a new <see cref="IAsyncValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="profileConstructor">Delegate that returns the validation profiles to include in the validator</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddAsyncValidator<TEntity, TError>(this IServiceCollection services, Func<IServiceProvider, IEnumerable<ValidationProfile<TError>>> profileConstructor, bool contextIsRequired = false)
        {
            services.ValidateArgument(nameof(services));
            profileConstructor.ValidateArgument(nameof(profileConstructor));

            return services.AddAsyncValidatorWithContext<TEntity, TError, object>(profileConstructor, contextIsRequired);
        }

        /// <summary>
        /// Registers a new <see cref="IAsyncValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error returned by the profiles</typeparam>
        /// <typeparam name="TNewError">Type of validation error returned by the validator</typeparam>
        /// <typeparam name="TContext">Type of context that can be used to modify the behaviour of the validator</typeparam>
        /// <typeparam name="TProfile">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="errorProjector">Delegate that converts the errors returned by the validation profiles to the intended error type</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddAsyncValidatorWithContext<TEntity, TError, TNewError, TContext, TProfile>(this IServiceCollection services, Func<ValidationError<TError>, TNewError> errorProjector, bool contextIsRequired = false) where TProfile : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));
            errorProjector.ValidateArgument(nameof(errorProjector));

            services.Register<IAsyncValidator<TEntity, TNewError, TContext>, DynamicValidator<TEntity, TError, TNewError, TContext>>(x =>
            {
                var profile = x.GetRequiredService<TProfile>();

                return new DynamicValidator<TEntity, TError, TNewError, TContext>(errorProjector, profile, contextIsRequired, x.GetService<ILogger<DynamicValidator<TEntity, TError, TNewError, TContext>>>());
            }, ServiceLifetime.Scoped);

            return services;
        }

        /// <summary>
        /// Registers a new <see cref="IAsyncValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error returned by the profiles</typeparam>
        /// <typeparam name="TNewError">Type of validation error returned by the validator</typeparam>
        /// <typeparam name="TProfile">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="errorProjector">Delegate that converts the errors returned by the validation profiles to the intended error type</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddAsyncValidator<TEntity, TError, TNewError, TProfile>(this IServiceCollection services, Func<ValidationError<TError>, TNewError> errorProjector, bool contextIsRequired = false) where TProfile : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));
            errorProjector.ValidateArgument(nameof(errorProjector));

            return services.AddAsyncValidatorWithContext<TEntity, TError, TNewError, object, TProfile>(errorProjector, contextIsRequired);
        }

        /// <summary>
        /// Registers a new <see cref="IAsyncValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error returned by the profiles</typeparam>
        /// <typeparam name="TNewError">Type of validation error returned by the validator</typeparam>
        /// <typeparam name="TContext">Type of context that can be used to modify the behaviour of the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="errorProjector">Delegate that converts the errors returned by the validation profiles to the intended error type</param>
        /// <param name="profileConstructor">Delegate that returns the validation profiles to include in the validator</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddAsyncValidatorWithContext<TEntity, TError, TNewError, TContext>(this IServiceCollection services, Func<ValidationError<TError>, TNewError> errorProjector, Func<IServiceProvider, IEnumerable<ValidationProfile<TError>>> profileConstructor, bool contextIsRequired = false)
        {
            services.ValidateArgument(nameof(services));
            errorProjector.ValidateArgument(nameof(errorProjector));
            profileConstructor.ValidateArgument(nameof(profileConstructor));

            services.Register<IAsyncValidator<TEntity, TNewError, TContext>, DynamicValidator<TEntity, TError, TNewError, TContext>>(x =>
            {
                var profiles = profileConstructor(x) ?? throw new InvalidOperationException($"{nameof(profileConstructor)} returned null");

                return new DynamicValidator<TEntity, TError, TNewError, TContext>(errorProjector, contextIsRequired, x.GetService<ILogger<DynamicValidator<TEntity, TError, TNewError, TContext>>>(), profiles.ToArray());
            }, ServiceLifetime.Scoped);

            return services;
        }

        /// <summary>
        /// Registers a new <see cref="IAsyncValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error returned by the profiles</typeparam>
        /// <typeparam name="TNewError">Type of validation error returned by the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="errorProjector">Delegate that converts the errors returned by the validation profiles to the intended error type</param>
        /// <param name="profileConstructor">Delegate that returns the validation profiles to include in the validator</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddAsyncValidator<TEntity, TError, TNewError>(this IServiceCollection services, Func<ValidationError<TError>, TNewError> errorProjector, Func<IServiceProvider, IEnumerable<ValidationProfile<TError>>> profileConstructor, bool contextIsRequired = false)
        {
            services.ValidateArgument(nameof(services));
            errorProjector.ValidateArgument(nameof(errorProjector));
            profileConstructor.ValidateArgument(nameof(profileConstructor));

            return services.AddAsyncValidatorWithContext<TEntity, TError, TNewError, object>(errorProjector, profileConstructor, contextIsRequired);
        }
        #endregion

        #region Options
        /// <summary>
        /// Adds an option validator for type <typeparamref name="TOption"/> that using validation profiles to validate the option instances.
        /// </summary>
        /// <typeparam name="TOption">Type of the options to validate</typeparam>
        /// <param name="services">Service collection to add the service registration to</param>
        /// <param name="profileConstructor">Delegate that returns the validation profiles to use</param>
        /// <param name="executionOptions">The options to use when calling the validation profiles</param>
        /// <param name="projector">Optional projector to modify the error messages returned from the profiles. 
        /// The default projector prefixes the error message like {Property}: {ErrorMessage} if a property is available for the error message</param>
        /// <param name="targets">The names of the options instances the current validator can target. If set to null or empty all options will be validated</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddOptionProfileValidator<TOption>(this IServiceCollection services, Func<IServiceProvider, IEnumerable<ValidationProfile<string>>> profileConstructor, ProfileExecutionOptions executionOptions = ProfileExecutionOptions.None, Func<ValidationError<string>, string> projector = null, params string[] targets) where TOption : class
        {
            services.ValidateArgument(nameof(services));
            profileConstructor.ValidateArgument(nameof(profileConstructor));

            services.AddScoped<IValidateOptions<TOption>>(x =>
            {
                var profiles = profileConstructor(x);
                return new OptionsProfileValidator<TOption>(profiles, executionOptions, projector, x.GetService<ILogger<OptionsProfileValidator<TOption>>>(), targets);
            });

            return services;
        }

        /// <summary>
        /// Adds an option validator for type <typeparamref name="TOption"/> that uses validation profiles to validate the option instances.
        /// </summary>
        /// <typeparam name="TOption">Type of the options to validate</typeparam>
        /// <typeparam name="TProfile">Type of the validation profile to use</typeparam>
        /// <param name="services">Service collection to add the service registration to</param>
        /// <param name="executionOptions">The options to use when calling the validation profiles</param>
        /// <param name="projector">Optional projector to modify the error messages returned from the profiles. 
        /// The default projector prefixes the error message like {Property}: {ErrorMessage} if a property is available for the error message</param>
        /// <param name="targets">The names of the options instances the current validator can target. If set to null or empty all options will be validated</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddOptionProfileValidator<TOption, TProfile>(this IServiceCollection services, ProfileExecutionOptions executionOptions = ProfileExecutionOptions.None, Func<ValidationError<string>, string> projector = null, params string[] targets) 
            where TOption : class
            where TProfile : ValidationProfile<string>
        {
            services.ValidateArgument(nameof(services));

            return services.AddOptionProfileValidator<TOption>(x => x.GetRequiredService<TProfile>().AsEnumerable(), executionOptions, projector, targets);
        }
        #endregion
    }
}
