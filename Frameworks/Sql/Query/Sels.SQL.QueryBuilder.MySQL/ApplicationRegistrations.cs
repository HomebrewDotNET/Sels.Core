using Castle.Core.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.MySQL;
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
        /// Registers <see cref="ISqlCompiler"/> so it can be used by the query providers.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMySqlCompiler(this IServiceCollection services)
        {
            services.ValidateArgument(nameof(services));

            services.New<ISqlCompiler, MySqlCompiler>()
                    .ConstructWith(p =>
                    {
                        var loggerFactory = p.GetService<Logging.ILoggerFactory>();
                        return new MySqlCompiler(loggerFactory?.CreateLogger<MySqlCompiler>());
                    })
                    .Trace(x => x.Duration.OfAll)
                    .AsScoped()
                    .TryRegister();

            return services;
        }

        /// <summary>
        /// Registers <see cref="ISqlQueryProvider"/> that can be injected in repositories to generate MySQL queries.
        /// </summary>
        /// <param name="services">Collection to add the services to</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddMySqlQueryProvider(this IServiceCollection services)
        {
            services.ValidateArgument(nameof(services));

            services.AddSqlQueryProvider();
            services.AddMySqlCompiler();

            return services;
        }

        /// <summary>
        /// Registers <see cref="ICachedSqlQueryProvider"/> that can be injected in repositories to generate MySQL queries.
        /// </summary>
        /// <param name="memoryOptionsBuilder">Delegate for configuring the options for the cached queries</param>
        /// <param name="expressionCompileOptions">The default compile options for generated queries</param>
        /// <param name="services">Collection to add the services to</param>
        /// <param name="overwrite">True to overwrite previous registrations, otherwise false</param>
        /// <returns><paramref name="services"/> for method chaining</returns>
        public static IServiceCollection AddCachedMySqlQueryProvider(this IServiceCollection services, Action<MemoryCacheEntryOptions> memoryOptionsBuilder = null, ExpressionCompileOptions expressionCompileOptions = ExpressionCompileOptions.None, bool overwrite = false)
        {
            services.ValidateArgument(nameof(services));

            services.AddMySqlCompiler();
            // MySql always needs the separator 
            expressionCompileOptions |= ExpressionCompileOptions.AppendSeparator;
            services.AddCachedSqlQueryProvider(memoryOptionsBuilder, expressionCompileOptions, overwrite);

            return services;
        }
    }
}
