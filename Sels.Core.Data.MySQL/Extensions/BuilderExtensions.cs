using Sels.Core.Data.MySQL.Query.Expressions;
using Sels.Core.Data.SQL.Query;
using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.MySQL
{
    /// <summary>
    /// Contains MySql extension methods for query builders.
    /// </summary>
    public static class BuilderExtensions
    {
        /// <summary>
        /// Locks the selected rows for updating within the same transaction.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to select</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived ForUpdate<TEntity, TDerived>(this ISelectQueryBuilder<TEntity, TDerived> builder)
        {
            builder.ValidateArgument(nameof(builder));

            return builder.InnerExpressions.Any(x => x is ForUpdateExpression) ? builder.Instance : builder.Expression(ForUpdateExpression.Instance, SelectExpressionPositions.After);
        }

        #region Limit
        /// <summary>
        /// Limits the amount of rows returned.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to insert</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <param name="limit">Object containing the amount of rows to limit by</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived Limit<TEntity, TDerived>(this ISelectQueryBuilder<TEntity, TDerived> builder, object limit)
        {
            builder.ValidateArgument(nameof(builder));
            limit.ValidateArgument(nameof(limit));

            return Limit(builder, null, limit);
        }
        /// <summary>
        /// Limits the amount of rows returned.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to insert</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <param name="offset">Optional offset containing the amount of rows to skip</param>
        /// <param name="limit">Object containing the amount of rows to limit by</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived Limit<TEntity, TDerived>(this ISelectQueryBuilder<TEntity, TDerived> builder, object? offset, object limit)
        {
            builder.ValidateArgument(nameof(builder));
            limit.ValidateArgument(nameof(limit));

            var limitExpression = limit is IExpression lE ? lE : new ConstantExpression(limit);
            var offsetExprresion = offset != null ? offset is IExpression oE ? oE : new ConstantExpression(offset) : null;

            var expression = builder.InnerExpressions.FirstOrDefault(x => x is LimitOffsetExpression).CastOrDefault<LimitOffsetExpression>();

            // Update existing expression
            if (expression != null)
            {
                expression.LimitExpression = limitExpression;
                expression.OffsetExpression = offsetExprresion;
                return builder.Instance;
            }
            // Add new expression
            else
            {
                return builder.Expression(new LimitOffsetExpression(limitExpression, offsetExprresion), SelectExpressionPositions.After);
            }
        }
        #endregion

        #region Concat
        /// <summary>
        /// Concatenates the supplied values by using the MySql CONCAT function.
        /// </summary>
        /// <typeparam name="TReturn">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <param name="values">The expressions to supply to the CONCAT function as arguments</param>
        /// <returns>Builder for creating more expressions</returns>
        public static TReturn Concat<TEntity, TReturn>(this ISharedExpressionBuilder<TEntity, TReturn> builder, IEnumerable<object> values)
        {
            builder.ValidateArgument(nameof(builder));
            values.ValidateArgumentNotNullOrEmpty(nameof(values));
            values.GetCount().ValidateArgumentLarger($"{nameof(values)}.Count()", 1);

            return builder.Expression(new ConcatExpression(values.Select(x => x is IExpression e ? e : new ConstantExpression(x))));
        }
        /// <summary>
        /// Concatenates the supplied values by using the MySql CONCAT function.
        /// </summary>
        /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
        /// <typeparam name="TReturn">The type to return for the fluent syntax</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <param name="firstValue">The first value to concatenate</param>
        /// <param name="secondValue">The second value to concatenate</param>
        /// <param name="additionalValues">Optional additional values to concatenate</param>
        /// <returns>Builder for creating more expressions</returns>
        public static TReturn Concat<TEntity, TReturn>(this ISharedExpressionBuilder<TEntity, TReturn> builder, object firstValue, object secondValue, params object[] additionalValues)
        {
            builder.ValidateArgument(nameof(builder));
            firstValue.ValidateArgument(nameof(firstValue));
            secondValue.ValidateArgument(nameof(secondValue));

            return Concat(builder, Helper.Collection.EnumerateAll(firstValue.AsArray(), secondValue.AsArray(), additionalValues));
        }
        /// <summary>
        /// Creates a comparison where an expression is compared to the value of a sql parameter concatenated with wildcards.
        /// </summary>
        /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
        /// <param name="builder">The builder to add the expressions to</param>
        /// <param name="parameter">The name of the sql parameter</param>
        /// <returns>Builder for creating more expressions</returns>
        public static IChainedConditionBuilder<TEntity> LikeParameter<TEntity>(this IConditionOperatorExpressionBuilder<TEntity> builder, string parameter)
        {
            builder.ValidateArgument(nameof(builder));
            parameter.ValidateArgumentNotNullOrWhitespace(nameof(parameter));

            return builder.Like().Concat("%", parameter.AsParameterExpression(), "%");
        }
        #endregion


    }
}
