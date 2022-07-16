using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sels.Core.Components.IoC;
using Sels.Core.Contracts.Validation;
using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Components.Validators;
using Sels.ObjectValidationFramework.Templates.Profile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for registering services into a <see cref="IServiceCollection"/>
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
        /// <returns>Self</returns>
        public static IServiceCollection RegisterProfile<TProfile, TError>(this IServiceCollection services, ServiceScope scope = ServiceScope.Scoped) where TProfile : ValidationProfile<TError>
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
        /// <returns>Self</returns>
        public static IServiceCollection RegisterProfile<TProfile, TError>(this IServiceCollection services, Func<IServiceProvider, TProfile> profileFactory, ServiceScope scope = ServiceScope.Scoped) where TProfile : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            services.Register<ValidationProfile<TError>, TProfile>(profileFactory, scope);
            services.Register(profileFactory, scope);

            return services;
        }
        #endregion

        #region ValidatorWithContext
        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <typeparam name="TContext">Type of context that can be used to modify the behaviour of the validator</typeparam>
        /// <typeparam name="TProfile">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="registerProfile">If this method should register the profiles. Set to false if already registered in <paramref name="services"/></param>
        /// <param name="loggerFactory">Optional logger factory that created the loggers for the validator</param>
        /// <returns>Self</returns>
        public static IServiceCollection RegisterValidatorWithContext<TEntity, TError, TContext, TProfile>(this IServiceCollection services, ServiceScope scope = ServiceScope.Scoped, bool contextIsRequired = false, bool registerProfile = true, Func<IServiceProvider, IEnumerable<ILogger>> loggerFactory = null) where TProfile : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            if (registerProfile)
            {
                services.RegisterProfile<TProfile, TError>(scope);
            }

            services.Register<IValidator<TEntity, TError, TContext>, DynamicValidator<TEntity, TError, TContext>>(x =>
            {
                var profile = x.GetRequiredService<TProfile>();

                IEnumerable<ILogger> loggers = loggerFactory.HasValue() ? loggerFactory(x) : null;

                return new DynamicValidator<TEntity, TError, TContext>(profile, contextIsRequired, loggers);
            }, scope);

            return services;
        }

        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <typeparam name="TContext">Type of context that can be used to modify the behaviour of the validator</typeparam>
        /// <typeparam name="TProfileOne">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileTwo">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="registerProfile">If this method should register the profiles. Set to false if already registered in <paramref name="services"/></param>
        /// <param name="loggerFactory">Optional logger factory that created the loggers for the validator</param>
        /// <returns>Self</returns>
        public static IServiceCollection RegisterValidatorWithContext<TEntity, TError, TContext, TProfileOne, TProfileTwo>(this IServiceCollection services, ServiceScope scope = ServiceScope.Scoped, bool contextIsRequired = false, bool registerProfile = true, Func<IServiceProvider, IEnumerable<ILogger>> loggerFactory = null) 
            where TProfileOne : ValidationProfile<TError>
            where TProfileTwo : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            if (registerProfile)
            {
                services
                    .RegisterProfile<TProfileOne, TError>(scope)
                    .RegisterProfile<TProfileTwo, TError>(scope);
            }

            services.Register<IValidator<TEntity, TError, TContext>, DynamicValidator<TEntity, TError, TContext>>(x =>
            {
                var profiles = new ValidationProfile<TError>[]
                {
                    x.GetRequiredService<TProfileOne>(),
                    x.GetRequiredService<TProfileTwo>()
                };

                IEnumerable<ILogger> loggers = loggerFactory.HasValue() ? loggerFactory(x) : null;

                return new DynamicValidator<TEntity, TError, TContext>(contextIsRequired, loggers, profiles);
            }, scope);

            return services;
        }

        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <typeparam name="TContext">Type of context that can be used to modify the behaviour of the validator</typeparam>
        /// <typeparam name="TProfileOne">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileTwo">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileThree">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="registerProfile">If this method should register the profiles. Set to false if already registered in <paramref name="services"/></param>
        /// <param name="loggerFactory">Optional logger factory that created the loggers for the validator</param>
        /// <returns>Self</returns>
        public static IServiceCollection RegisterValidatorWithContext<TEntity, TError, TContext, TProfileOne, TProfileTwo, TProfileThree>(this IServiceCollection services, ServiceScope scope = ServiceScope.Scoped, bool contextIsRequired = false, bool registerProfile = true, Func<IServiceProvider, IEnumerable<ILogger>> loggerFactory = null)
            where TProfileOne : ValidationProfile<TError>
            where TProfileTwo : ValidationProfile<TError>
            where TProfileThree : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            if (registerProfile)
            {
                services
                    .RegisterProfile<TProfileOne, TError>(scope)
                    .RegisterProfile<TProfileTwo, TError>(scope)
                    .RegisterProfile<TProfileThree, TError>(scope);
            }

            services.Register<IValidator<TEntity, TError, TContext>, DynamicValidator<TEntity, TError, TContext>>(x =>
            {
                var profiles = new ValidationProfile<TError>[]
                {
                    x.GetRequiredService<TProfileOne>(),
                    x.GetRequiredService<TProfileTwo>(),
                    x.GetRequiredService<TProfileThree>()
                };

                IEnumerable<ILogger> loggers = loggerFactory.HasValue() ? loggerFactory(x) : null;

                return new DynamicValidator<TEntity, TError, TContext>(contextIsRequired, loggers, profiles);
            }, scope);

            return services;
        }

        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <typeparam name="TContext">Type of context that can be used to modify the behaviour of the validator</typeparam>
        /// <typeparam name="TProfileOne">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileTwo">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileThree">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileFour">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="registerProfile">If this method should register the profiles. Set to false if already registered in <paramref name="services"/></param>
        /// <param name="loggerFactory">Optional logger factory that created the loggers for the validator</param>
        /// <returns>Self</returns>
        public static IServiceCollection RegisterValidatorWithContext<TEntity, TError, TContext, TProfileOne, TProfileTwo, TProfileThree, TProfileFour>(this IServiceCollection services, ServiceScope scope = ServiceScope.Scoped, bool contextIsRequired = false, bool registerProfile = true, Func<IServiceProvider, IEnumerable<ILogger>> loggerFactory = null)
            where TProfileOne : ValidationProfile<TError>
            where TProfileTwo : ValidationProfile<TError>
            where TProfileThree : ValidationProfile<TError>
            where TProfileFour : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            if (registerProfile)
            {
                services
                    .RegisterProfile<TProfileOne, TError>(scope)
                    .RegisterProfile<TProfileTwo, TError>(scope)
                    .RegisterProfile<TProfileThree, TError>(scope)
                    .RegisterProfile<TProfileFour, TError>(scope);
            }

            services.Register<IValidator<TEntity, TError, TContext>, DynamicValidator<TEntity, TError, TContext>>(x =>
            {
                var profiles = new ValidationProfile<TError>[]
                {
                    x.GetRequiredService<TProfileOne>(),
                    x.GetRequiredService<TProfileTwo>(),
                    x.GetRequiredService<TProfileThree>(),
                    x.GetRequiredService<TProfileFour>()
                };

                IEnumerable<ILogger> loggers = loggerFactory.HasValue() ? loggerFactory(x) : null;

                return new DynamicValidator<TEntity, TError, TContext>(contextIsRequired, loggers, profiles);
            }, scope);

            return services;
        }

        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <typeparam name="TContext">Type of context that can be used to modify the behaviour of the validator</typeparam>
        /// <typeparam name="TProfileOne">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileTwo">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileThree">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileFour">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileFive">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="registerProfile">If this method should register the profiles. Set to false if already registered in <paramref name="services"/></param>
        /// <param name="loggerFactory">Optional logger factory that created the loggers for the validator</param>
        /// <returns>Self</returns>
        public static IServiceCollection RegisterValidatorWithContext<TEntity, TError, TContext, TProfileOne, TProfileTwo, TProfileThree, TProfileFour, TProfileFive>(this IServiceCollection services, ServiceScope scope = ServiceScope.Scoped, bool contextIsRequired = false, bool registerProfile = true, Func<IServiceProvider, IEnumerable<ILogger>> loggerFactory = null)
            where TProfileOne : ValidationProfile<TError>
            where TProfileTwo : ValidationProfile<TError>
            where TProfileThree : ValidationProfile<TError>
            where TProfileFour : ValidationProfile<TError>
            where TProfileFive : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            if (registerProfile)
            {
                services
                    .RegisterProfile<TProfileOne, TError>(scope)
                    .RegisterProfile<TProfileTwo, TError>(scope)
                    .RegisterProfile<TProfileThree, TError>(scope)
                    .RegisterProfile<TProfileFour, TError>(scope)
                    .RegisterProfile<TProfileFive, TError>(scope);
            }

            services.Register<IValidator<TEntity, TError, TContext>, DynamicValidator<TEntity, TError, TContext>>(x =>
            {
                var profiles = new ValidationProfile<TError>[]
                {
                    x.GetRequiredService<TProfileOne>(),
                    x.GetRequiredService<TProfileTwo>(),
                    x.GetRequiredService<TProfileThree>(),
                    x.GetRequiredService<TProfileFour>(),
                    x.GetRequiredService<TProfileFive>()
                };

                IEnumerable<ILogger> loggers = loggerFactory.HasValue() ? loggerFactory(x) : null;

                return new DynamicValidator<TEntity, TError, TContext>(contextIsRequired, loggers, profiles);
            }, scope);

            return services;
        }
        #endregion

        #region Validator
        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <typeparam name="TProfile">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="registerProfile">If this method should register the profiles. Set to false if already registered in <paramref name="services"/></param>
        /// <param name="loggerFactory">Optional logger factory that created the loggers for the validator</param>
        /// <returns>Self</returns>
        public static IServiceCollection RegisterValidator<TEntity, TError, TProfile>(this IServiceCollection services, ServiceScope scope = ServiceScope.Scoped, bool contextIsRequired = false, bool registerProfile = true, Func<IServiceProvider, IEnumerable<ILogger>> loggerFactory = null) where TProfile : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            return services.RegisterValidatorWithContext<TEntity, TError, object, TProfile>(scope, contextIsRequired, registerProfile, loggerFactory);
        }

        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <typeparam name="TProfileOne">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileTwo">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="registerProfile">If this method should register the profiles. Set to false if already registered in <paramref name="services"/></param>
        /// <param name="loggerFactory">Optional logger factory that created the loggers for the validator</param>
        /// <returns>Self</returns>
        public static IServiceCollection RegisterValidator<TEntity, TError, TProfileOne, TProfileTwo>(this IServiceCollection services, ServiceScope scope = ServiceScope.Scoped, bool contextIsRequired = false, bool registerProfile = true, Func<IServiceProvider, IEnumerable<ILogger>> loggerFactory = null)
            where TProfileOne : ValidationProfile<TError>
            where TProfileTwo : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            return services.RegisterValidatorWithContext<TEntity, TError, object, TProfileOne, TProfileTwo>(scope, contextIsRequired, registerProfile, loggerFactory);
        }

        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <typeparam name="TProfileOne">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileTwo">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileThree">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="registerProfile">If this method should register the profiles. Set to false if already registered in <paramref name="services"/></param>
        /// <param name="loggerFactory">Optional logger factory that created the loggers for the validator</param>
        /// <returns>Self</returns>
        public static IServiceCollection RegisterValidator<TEntity, TError, TProfileOne, TProfileTwo, TProfileThree>(this IServiceCollection services, ServiceScope scope = ServiceScope.Scoped, bool contextIsRequired = false, bool registerProfile = true, Func<IServiceProvider, IEnumerable<ILogger>> loggerFactory = null)
            where TProfileOne : ValidationProfile<TError>
            where TProfileTwo : ValidationProfile<TError>
            where TProfileThree : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            return services.RegisterValidatorWithContext<TEntity, TError, object, TProfileOne, TProfileTwo, TProfileThree>(scope, contextIsRequired, registerProfile, loggerFactory);
        }

        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <typeparam name="TProfileOne">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileTwo">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileThree">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileFour">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="registerProfile">If this method should register the profiles. Set to false if already registered in <paramref name="services"/></param>
        /// <param name="loggerFactory">Optional logger factory that created the loggers for the validator</param>
        /// <returns>Self</returns>
        public static IServiceCollection RegisterValidator<TEntity, TError, TProfileOne, TProfileTwo, TProfileThree, TProfileFour>(this IServiceCollection services, ServiceScope scope = ServiceScope.Scoped, bool contextIsRequired = false, bool registerProfile = true, Func<IServiceProvider, IEnumerable<ILogger>> loggerFactory = null)
            where TProfileOne : ValidationProfile<TError>
            where TProfileTwo : ValidationProfile<TError>
            where TProfileThree : ValidationProfile<TError>
            where TProfileFour : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            return services.RegisterValidatorWithContext<TEntity, TError, object, TProfileOne, TProfileTwo, TProfileThree, TProfileFour>(scope, contextIsRequired, registerProfile, loggerFactory);
        }

        /// <summary>
        /// Registers a new <see cref="IValidator{TEntity, TError, TContext}"/> that uses the provided profiles to validate entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <typeparam name="TProfileOne">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileTwo">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileThree">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileFour">Type of profile to use for the validator</typeparam>
        /// <typeparam name="TProfileFive">Type of profile to use for the validator</typeparam>
        /// <param name="services">Service collection to add service to</param>
        /// <param name="scope">Which scope to use for the service</param>
        /// <param name="contextIsRequired">If a context is required by this validator. If set to true and context is null the service will throw an <see cref="ArgumentNullException"/></param>
        /// <param name="registerProfile">If this method should register the profiles. Set to false if already registered in <paramref name="services"/></param>
        /// <param name="loggerFactory">Optional logger factory that created the loggers for the validator</param>
        /// <returns>Self</returns>
        public static IServiceCollection RegisterValidator<TEntity, TError, TProfileOne, TProfileTwo, TProfileThree, TProfileFour, TProfileFive>(this IServiceCollection services, ServiceScope scope = ServiceScope.Scoped, bool contextIsRequired = false, bool registerProfile = true, Func<IServiceProvider, IEnumerable<ILogger>> loggerFactory = null)
            where TProfileOne : ValidationProfile<TError>
            where TProfileTwo : ValidationProfile<TError>
            where TProfileThree : ValidationProfile<TError>
            where TProfileFour : ValidationProfile<TError>
            where TProfileFive : ValidationProfile<TError>
        {
            services.ValidateArgument(nameof(services));

            return services.RegisterValidatorWithContext<TEntity, TError, object, TProfileOne, TProfileTwo, TProfileThree, TProfileFour, TProfileFive>(scope, contextIsRequired, registerProfile, loggerFactory);
        }
        #endregion
    }
}
