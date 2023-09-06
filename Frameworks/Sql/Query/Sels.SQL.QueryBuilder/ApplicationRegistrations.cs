using Castle.Core.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sels.Core.Extensions;
using Sels.Core.ServiceBuilder;
using Sels.SQL.QueryBuilder;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.Provider;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for registering services.
    /// </summary>
    public static class ApplicationRegistrations
    {
        private static IServiceCollection AddSqlQueryProviderOptions(this IServiceCollection services)
        {
            services.ValidateArgument(nameof(services));

            services.AddOptions();

            services.BindOptionsFromConfig<SqlQueryProviderOptions>(nameof(SqlQueryProviderOptions));
            services.AddOptionProfileValidator<SqlQueryProviderOptions, SqlQueryProviderOptionsValidationProfile>();
            services.AddValidationProfile<SqlQueryProviderOptionsValidationProfile, string>(ServiceLifetime.Singleton);

            return services;
        }

        /// <summary>
        /// Registers <see cref="ISqlQueryProvider"/> that can be injected in repositories to generate sql queries.
        /// </summary>
        /// <param name="services">Collection to add the services to</param>
        /// <param name="overwrite">True to overwrite previous registrations, otherwise false</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddSqlQueryProvider(this IServiceCollection services, bool overwrite = false)
        {
            services.ValidateArgument(nameof(services));

            services.AddSqlQueryProviderOptions();

            services.New<ISqlQueryProvider, SqlQueryProvider>()
                    .Trace((p, b) =>
                    {
                        var options = p.GetRequiredService<IOptions<SqlQueryProviderOptions>>();
                        return b.Duration.OfAll.WithDurationThresholds(options.Value.PerformanceWarningDurationThreshold, options.Value.PerformanceErrorDurationThreshold);
                    })
                    .AsScoped()
                    .WithBehaviour(overwrite ? services.IsReadOnly ? RegisterBehaviour.Default : RegisterBehaviour.Replace : RegisterBehaviour.TryAdd)
                    .Register();

            return services;
        }

        /// <summary>
        /// Registers <see cref="ICachedSqlQueryProvider"/> that can be injected in repositories to generate sql queries.
        /// </summary>
        /// <param name="services">Collection to add the services to</param>
        /// <param name="overwrite">True to overwrite previous registrations, otherwise false</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddCachedSqlQueryProvider(this IServiceCollection services, bool overwrite = false)
        {
            services.ValidateArgument(nameof(services));

            services.AddSqlQueryProviderOptions();

            services.AddMemoryCache();

            services.New<ICachedSqlQueryProvider, CachedSqlQueryProvider>()
                    .ConstructWith(p => {
                        var options = p.GetRequiredService<IOptions<SqlQueryProviderOptions>>();
                        return new CachedSqlQueryProvider(p.GetRequiredService<IMemoryCache>(), options.Value.CacheEntryOptions, p.GetRequiredService<ISqlCompiler>(), options.Value.CompileOptions, p.GetService<ILogger<CachedSqlQueryProvider>>());
                    })
                    .Trace((p, b) =>
                    {
                        var options = p.GetRequiredService<IOptions<SqlQueryProviderOptions>>();
                        return b.Duration.OfAll.WithDurationThresholds(options.Value.PerformanceWarningDurationThreshold, options.Value.PerformanceErrorDurationThreshold);
                    })
                    .AsScoped()
                    .WithBehaviour(overwrite ? services.IsReadOnly ? RegisterBehaviour.Default : RegisterBehaviour.Replace : RegisterBehaviour.TryAdd)
                    .Register();

            return services;
        }
    }
}
