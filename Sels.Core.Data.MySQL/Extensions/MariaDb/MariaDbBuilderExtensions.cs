using Sels.Core.Data.MySQL.Query.Expressions.MariaDb;
using Sels.Core.Data.SQL.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.MySQL.MariaDb
{
    /// <summary>
    /// Contains MariaDb specific MySql extension methods for query builders.
    /// </summary>
    public static class MariaDbBuilderExtensions
    {
        #region Returning
        /// <summary>
        /// Specifies what to return after deleting.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to delete</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <param name="configurator">Delegate used to configure what to return</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived Return<TEntity, TDerived>(this IDeleteQueryBuilder<TEntity, TDerived> builder, Action<IReturningExpressionBuilder<TEntity>> configurator)
        {
            builder.ValidateArgument(nameof(builder));
            configurator.ValidateArgument(nameof(configurator));

            return builder.Returning(configurator, DeleteExpressionPositions.After);
        }
        /// <summary>
        /// Specifies what to return after inserting.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to insert</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <param name="configurator">Delegate used to configure what to return</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived Return<TEntity, TDerived>(this IInsertQueryBuilder<TEntity, TDerived> builder, Action<IReturningExpressionBuilder<TEntity>> configurator)
        {
            builder.ValidateArgument(nameof(builder));
            configurator.ValidateArgument(nameof(configurator));

            return builder.Returning(configurator, InsertExpressionPositions.After);
        }
        private static TDerived Returning<TEntity, TPosition, TDerived>(this IQueryBuilder<TEntity, TPosition, TDerived> builder, Action<IReturningExpressionBuilder<TEntity>> configurator, TPosition position)
        {
            builder.ValidateArgument(nameof(builder));
            configurator.ValidateArgument(nameof(configurator));
            position.ValidateArgument(nameof(position));

            return builder.Expression(new ReturningExpression<TEntity>(configurator), position);
        }
        #endregion
    }
}
