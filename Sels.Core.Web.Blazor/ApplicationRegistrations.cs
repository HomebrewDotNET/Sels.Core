using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Sels.Core.Contracts.Configuration;
using Sels.Core.Web.Blazor;
using Sels.Core.Web.Blazor.Authentication;
using Sels.Core.Web.Blazor.Authentication.Token;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for registering services
    /// </summary>
    public static class ApplicationRegistrations
    {
        /// <summary>
        /// Registers the jwt token service where the settings are loading in from the jwt section.
        /// </summary>
        /// <param name="services">Collection to add the services to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddJwtTokenService(this IServiceCollection services)
        {
            services.ValidateArgument(nameof(services));

            services.New<ITokenService, JwtTokenService>().Trace(x => x.Duration.OfAll).ConstructWith(x =>
            {
                var configurationService = x.GetRequiredService<IConfigurationService>();

                return new JwtTokenService(
                    configurationService.Get(BlazorConstants.Config.Jwt.Issuer, x => x.FromSection(BlazorConstants.Config.Jwt.Section), ConfigurationSettings.Required),
                    configurationService.Get(BlazorConstants.Config.Jwt.Audience, x => x.FromSection(BlazorConstants.Config.Jwt.Section), ConfigurationSettings.Required),
                    configurationService.Get(BlazorConstants.Config.Jwt.Secret, x => x.FromSection(BlazorConstants.Config.Jwt.Section), ConfigurationSettings.Required).GetBytes(),
                    x.GetService<ILogger<JwtTokenService>>()
                    );
            }).AsSingleton().Register();

            return services;
        }

        /// <summary>
        /// Adds an authentication provider that manages tokens in the browser storage.
        /// </summary>
        /// <param name="services">Collection to add the services to</param>
        /// <param name="storageKey">The key where the tokens are saved</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddTokenStorageAuthenticationProvider(this IServiceCollection services, string storageKey)
        {
            services.ValidateArgument(nameof(services));
            storageKey.ValidateArgumentNotNullOrWhitespace(nameof(storageKey));

            var factory = new Func<IServiceProvider, TokenStorageAuthenticationService>(x =>
            {
                return new TokenStorageAuthenticationService(
                    x.GetRequiredService<ITokenService>(),
                    x.GetRequiredService<ILocalStorageService>(),
                    storageKey,
                    x.GetService<ILogger<TokenStorageAuthenticationService>>()
                    );
            });

            services.New<AuthenticationStateProvider, TokenStorageAuthenticationService>().Trace(x => x.Duration.OfAll).ConstructWith(factory).AsScoped().ForceRegister();
            services.New<IAuthenticationService, TokenStorageAuthenticationService>().Trace(x => x.Duration.OfAll).ConstructWith(factory).AsScoped().Register();

            return services;
        }
    }
}
