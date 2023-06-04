using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builder for creating an sql query that consists of multiple statements and/or expressions.
    /// </summary>
    public interface IMultiStatementBuilder : IQueryBuilder
    {
        /// <summary>
        /// Adds <paramref name="builder"/> so it can be compiled into sql.
        /// </summary>
        /// <param name="builder">The builder to add</param>
        /// <returns>Current builder for method chaining</returns>
        IMultiStatementBuilder Append(IQueryBuilder builder);
        /// <summary>
        /// Adds <paramref name="expression"/> so it can be compiled into sql.
        /// </summary>
        /// <param name="expression">The expression to add</param>
        /// <param name="isFullStatement">Whether or not <paramref name="expression"/> compiles to a full sql statement. Is used to handle compilation options like <see cref="ExpressionCompileOptions.AppendSeparator"/> and <see cref="ExpressionCompileOptions.Format"/></param>
        /// <returns>Current builder for method chaining</returns>
        IMultiStatementBuilder Append(IExpression expression, bool isFullStatement = true);
        /// <summary>
        /// Adds <paramref name="rawSql"/> to the builder.
        /// </summary>
        /// <param name="rawSql">String containing raw sql</param>
        /// <param name="isFullStatement">Whether or not <paramref name="rawSql"/> compiles to a full sql statement. Is used to handle compilation options like <see cref="ExpressionCompileOptions.AppendSeparator"/> and <see cref="ExpressionCompileOptions.Format"/></param>
        /// <returns>Current builder for method chaining</returns>
        IMultiStatementBuilder Append(string rawSql, bool isFullStatement = true) => Append(new RawExpression(rawSql.ValidateArgument(nameof(rawSql))), isFullStatement);
    }
}
