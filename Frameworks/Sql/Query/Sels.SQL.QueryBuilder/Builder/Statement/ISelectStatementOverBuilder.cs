using Sels.Core;
using Sels.Core.Extensions;
using Sels.Core.Models;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Expressions;
using Sels.SQL.QueryBuilder.Expressions.Over;
using Sels.SQL.QueryBuilder.Models.Expressions.Over;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builder for creating an OVER clause.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public interface ISelectStatementOverBuilder<TEntity> : 
        IOrderByBuilder<TEntity, 
        ISelectStatementOverBuilder<TEntity>>
    {
        #region Partition
        /// <summary>
        /// Adds an expression that defines how to partition the result set.
        /// </summary>
        /// <param name="expression">The expression to add</param>
        /// <returns>Current builder for method chaining</returns>
        ISelectStatementOverBuilder<TEntity> PartitionBy(IExpression expression);
        /// <summary>
        /// Adds an expression created by <paramref name="expressionBuilder"/> that defines how to partition the result set.
        /// </summary>
        /// <param name="expressionBuilder">Delegate that selects the expression to add</param>
        /// <param name="additionalBuilders">Optional extra builders for defining more values to partition by</param>
        /// <returns>Current builder for method chaining</returns>
        ISelectStatementOverBuilder<TEntity> PartitionBy(Action<ISharedExpressionBuilder<TEntity, Null>> expressionBuilder, params Action<ISharedExpressionBuilder<TEntity, Null>>[] additionalBuilders)
        {
            expressionBuilder.ValidateArgument(nameof(expressionBuilder));

            return Helper.Collection.Enumerate(expressionBuilder, additionalBuilders).Where(x => x != null).Select(x => PartitionBy(new ExpressionBuilder<TEntity>(x))).Last();
        }
        /// <summary>
        /// Adds an expression with which columns to partition by.
        /// </summary>
        /// <param name="column">The column to partition by</param>
        /// <param name="additionalColumns">Optional extra columns to partition by</param>
        /// <returns>Current builder for method chaining</returns>
        ISelectStatementOverBuilder<TEntity> PartitionBy(string column, params string[] additionalColumns)
        {
            column.ValidateArgument(nameof(column));

            return Helper.Collection.Enumerate(column, additionalColumns).Where(x => x != null).Select(x => PartitionBy(new ColumnExpression(null, x))).Last();
        }
        /// <summary>
        /// Adds an expression with which columns to partition by.
        /// </summary>
        /// <param name="dataSetAlias">Optional dataset alias to select column from</param>
        /// <param name="column">The column to partition by</param>
        /// <param name="additionalColumns">Optional extra columns to partition by</param>
        /// <returns>Current builder for method chaining</returns>
        ISelectStatementOverBuilder<TEntity> PartitionBy(object dataSetAlias, string column, params string[] additionalColumns)
        {
            column.ValidateArgument(nameof(column));

            return Helper.Collection.Enumerate(column, additionalColumns).Where(x => x != null).Select(x => PartitionBy(new ColumnExpression(dataSetAlias, x))).Last();
        }
        #endregion

        #region Window Frame
        /// <summary>
        /// Limits the window frame of the parition using <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">Expression that limits the window frame</param>
        /// <returns>Current builder for method chaining</returns>
        ISelectStatementOverBuilder<TEntity> WindowFrame(IExpression expression);

        /// <summary>
        /// Limits the window frame by using <paramref name="expression"/> that dictates how to limit the frame.
        /// </summary>
        /// <param name="expression">The expression that contains the limit</param>
        /// <returns>Builder for selecting the window frame boundries</returns>
        ISelectStatementOverFrameBorderBuilder<TEntity> LimitBy(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            var builder = new WindowFrameExpressionBuilder<TEntity>(this, expression);
            WindowFrame(builder);
            return builder;
        }
        /// <summary>
        /// Limits the window frame by using the <see cref="Sql.Over.Rows"/> clause.
        /// </summary>
        /// <returns>Builder for selecting the window frame boundries</returns>
        ISelectStatementOverFrameBorderBuilder<TEntity> Rows => LimitBy(new EnumExpression<WindowFrameLimits>(WindowFrameLimits.Rows));
        /// <summary>
        /// Limits the window frame by using the <see cref="Sql.Over.Range"/> clause.
        /// </summary>
        /// <returns>Builder for selecting the window frame boundries</returns>
        ISelectStatementOverFrameBorderBuilder<TEntity> Range => LimitBy(new EnumExpression<WindowFrameLimits>(WindowFrameLimits.Range));
        /// <summary>
        /// Limits the window frame by using the <see cref="Sql.Over.Groups"/> clause.
        /// </summary>
        /// <returns>Builder for selecting the window frame boundries</returns>
        ISelectStatementOverFrameBorderBuilder<TEntity> Groups => LimitBy(new EnumExpression<WindowFrameLimits>(WindowFrameLimits.Groups));
        #endregion
    }

    /// <summary>
    /// Builder for limiting a window frame border.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public interface ISelectStatementOverFrameBorderBuilder<TEntity> : ISelectStatementOverSharedFrameBorderBuilder<TEntity, ISelectStatementOverBuilder<TEntity>>
    {
        /// <summary>
        /// Returns a builder for selecting the lower and upper bound of the window frame.
        /// </summary>
        /// <returns>Builder for selecting the lower bound</returns>
        ISelectStatementOverSharedFrameBorderBuilder<TEntity, ISelectStatementOverIntermediateFrameBorderBuilder<TEntity>> Between { get; }
    }

    /// <summary>
    /// Builder for limiting a window frame within an OVER clause.
    /// Intermediate builder between selecting the lower and upper bound of the window frame.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public interface ISelectStatementOverIntermediateFrameBorderBuilder<TEntity>
    {
        /// <summary>
        /// Returns the builder for selecting the window frame upper bound.
        /// </summary>
        ISelectStatementOverSharedFrameBorderBuilder<TEntity, ISelectStatementOverBuilder<TEntity>> And { get; }
    }

    /// <summary>
    /// Builder for limiting a window frame within an OVER clause.
    /// Builder contains expression that can be used both in the lower and upper bound.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    /// <typeparam name="TReturn">The type to return</typeparam>
    public interface ISelectStatementOverSharedFrameBorderBuilder<TEntity, TReturn>
    {
        /// <summary>
        /// Adds an expression that sets the current bound.
        /// </summary>
        /// <param name="expression">Expression that defines the lower bound of the window function</param>
        /// <returns>Current builder for method chaining</returns>
        TReturn Expression(IExpression expression);
        /// <summary>
        /// Sets the current bound to the current row.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        TReturn CurrentRow() => Expression(new WindowFrameBorderCurrentRowExpression());
        /// <summary>
        /// Sets the upper bound to unbounded.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        TReturn UnboundedFollowing() => Expression(new WindowFrameBorderExpression(null, true));
        /// <summary>
        /// Sets the upper bound to <paramref name="count"/>.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        TReturn Following(uint count) => Expression(new WindowFrameBorderExpression(count, true));
        /// <summary>
        /// Sets the lower bound to unbounded.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        TReturn UnboundedPreceding() => Expression(new WindowFrameBorderExpression(null, false));
        /// <summary>
        /// Sets the lower bound to <paramref name="count"/>.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        TReturn Preceding(uint count) => Expression(new WindowFrameBorderExpression(count, false));
    }
}
