using Castle.Core.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
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
        /// <summary>
        /// Registers <see cref="ISqlQueryProvider"/> that can be injected in repositories to generate sql queries.
        /// </summary>
        /// <param name="services">Collection to add the services to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddSqlQueryProvider(this IServiceCollection services)
        {
            services.ValidateArgument(nameof(services));

            services.New<ISqlQueryProvider, SqlQueryProvider>()
                    .Trace(x => x.Duration.OfAll)
                    .AsScoped()
                    .TryRegister();

            return services;
        }

        /// <summary>
        /// Registers <see cref="ICachedSqlQueryProvider"/> that can be injected in repositories to generate sql queries.
        /// </summary>
        /// <param name="memoryOptionsBuilder">Delegate for configuring the options for the cached queries</param>
        /// <param name="expressionCompileOptions">The default compile options for generated queries</param>
        /// <param name="services">Collection to add the services to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddCachedSqlQueryProvider(this IServiceCollection services, Action<MemoryCacheEntryOptions> memoryOptionsBuilder = null, ExpressionCompileOptions expressionCompileOptions = ExpressionCompileOptions.None)
        {
            services.ValidateArgument(nameof(services));

            services.New<ICachedSqlQueryProvider, CachedSqlQueryProvider>()
                    .ConstructWith(p => {
                        var memoryOptions = new MemoryCacheEntryOptions();
                        memoryOptionsBuilder?.Invoke(memoryOptions);
                        return new CachedSqlQueryProvider(p.GetRequiredService<IMemoryCache>(), memoryOptions, p.GetRequiredService<ISqlCompiler>(), expressionCompileOptions, p.GetService<ILogger<CachedSqlQueryProvider>>());
                    })
                    .Trace(x => x.Duration.OfAll)
                    .AsScoped()
                    .TryRegister();

            return services;
        }
    }
}
