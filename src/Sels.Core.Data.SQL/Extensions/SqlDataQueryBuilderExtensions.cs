using Dapper;
using Microsoft.Extensions.Logging;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using Sels.Core.Data.SQL.SearchCriteria;
using System;
using Sels.Core.Extensions;

namespace Sels.Core.Data.SQL
{
    /// <summary>
    /// Contains extension methods for working with the sql builders.
    /// </summary>
    public static class SqlDataQueryBuilderExtensions
    {
        #region Search Criteria
        /// <summary>
        /// Converts <paramref name="searchCriteria"/> to SQL conditions.
        /// </summary>
        /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
        /// <typeparam name="TSearchCriteria">Type of the search criteria</typeparam>
        /// <param name="builder">The builder to create the conditions with</param>
        /// <param name="searchCriteria">THe object to convert to SQL conditions</param>
        /// <param name="configurator">Optional delegate for configuring how to convert <paramref name="searchCriteria"/></param>
        /// <param name="parameters">Optional parameter bag that can be provided. Implicit conditions that use parameters will automatically add the values to the bag</param>
        /// <param name="logger">Optional logger for debugging</param>
        /// <returns>The final builder after the creating the conditions or null if no conditions were created</returns>
        public static IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>>? FromSearchCriteria<TEntity, TSearchCriteria>(this IStatementConditionExpressionBuilder<TEntity> builder, TSearchCriteria searchCriteria, Action<ISearchCriteriaConverterBuilder<TEntity, TSearchCriteria>>? configurator = null, DynamicParameters? parameters = null, ILogger? logger = null)
        {
            builder.ValidateArgument(nameof(builder));
            searchCriteria.ValidateArgument(nameof(searchCriteria));

            var converter = new SearchCriteriaConverter<TEntity, TSearchCriteria>(configurator, logger);
            return converter.Build(builder, searchCriteria, parameters);
        }

        /// <summary>
        /// Converts <paramref name="searchCriteria"/> to SQL conditions.
        /// </summary>
        /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
        /// <typeparam name="TDerived">The type of the builder to create the conditions for</typeparam>
        /// <typeparam name="TSearchCriteria">Type of the search criteria</typeparam>
        /// <param name="builder">The builder to create the conditions with</param>
        /// <param name="searchCriteria">THe object to convert to SQL conditions</param>
        /// <param name="configurator">Optional delegate for configuring how to convert <paramref name="searchCriteria"/></param>
        /// <param name="parameters">Optional parameter bag that can be provided. Implicit conditions that use parameters will automatically add the values to the bag</param>
        /// <param name="logger">Optional logger for debugging</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived FromSearchCriteria<TEntity, TDerived, TSearchCriteria>(this IStatementConditionBuilder<TEntity, TDerived> builder, TSearchCriteria searchCriteria, Action<ISearchCriteriaConverterBuilder<TEntity, TSearchCriteria>> configurator = null, DynamicParameters parameters = null, ILogger? logger = null)
        {
            builder.ValidateArgument(nameof(builder));
            searchCriteria.ValidateArgument(nameof(searchCriteria));

            return builder.Where(x => x.FromSearchCriteria(searchCriteria, configurator, parameters, logger));
        }
        #endregion
    }
}
