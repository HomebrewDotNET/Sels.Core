using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using Sels.SQL.QueryBuilder.Extensions;
using Sels.SQL.QueryBuilder.MySQL.Builder.Statement;
using Sels.SQL.QueryBuilder.MySQL.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using Sels.Core.Extensions.Linq;
using Sels.Core;

namespace Sels.SQL.QueryBuilder.MySQL
{
    /// <summary>
    /// Contains MySql extension methods for query builders.
    /// </summary>
    public static class MySqlBuilderExtensions
    {
        /// <summary>
        /// Locks the selected rows for updating within the same transaction.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to select</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived ForUpdate<TEntity, TDerived>(this ISelectStatementBuilder<TEntity, TDerived> builder)
        {
            builder.ValidateArgument(nameof(builder));

            return builder.InnerExpressions.Any(x => x is ForUpdateExpression) ? builder.Instance : builder.Expression(ForUpdateExpression.Instance, SelectExpressionPositions.After, 1);
        }
        /// <summary>
        /// Locks the selected rows for updating within the same transaction but skip rows that are already locked.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to select</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived ForUpdateSkipLocked<TEntity, TDerived>(this ISelectStatementBuilder<TEntity, TDerived> builder)
        {
            builder.ValidateArgument(nameof(builder));

            return builder.InnerExpressions.Any(x => x is ForUpdateExpressionSkipLocked) ? builder.Instance : builder.Expression(ForUpdateExpressionSkipLocked.Instance, SelectExpressionPositions.After, 1);
        }

        #region OnDuplicateKeyUpdate
        /// <summary>
        /// Update records if there are duplicate primary keys in the insert statement.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to insert</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <param name="expressionBuilder">Delegate for building what to update on duplicate key</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived OnDuplicateKeyUpdate<TEntity, TDerived>(this IInsertStatementBuilder<TEntity, TDerived> builder, Action<IOnDuplicateKeyUpdateBuilder<TEntity>> expressionBuilder)
        {
            builder.ValidateArgument(nameof(builder));
            expressionBuilder.ValidateArgument(nameof(expressionBuilder));

            var expression = builder.InnerExpressions.FirstOrDefault(x => x is OnDuplicateKeyUpdateExpression<TEntity>).CastToOrDefault<OnDuplicateKeyUpdateExpression<TEntity>>();

            if(expression != null)
            {
                expression.Build(expressionBuilder);
                return builder.Instance;
            }
            else
            {
                return builder.Expression(new OnDuplicateKeyUpdateExpression<TEntity>(expressionBuilder), InsertExpressionPositions.After);
            }
        }
        /// <summary>
        /// Update records if there are duplicate primary keys in the insert statement.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to insert</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <param name="primaryKeyColumnIndexes">The indexes of the columns containing the primary keys, they will be omitted from the update expressions</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived OnDuplicateKeyUpdate<TEntity, TDerived>(this IInsertStatementBuilder<TEntity, TDerived> builder, params int[] primaryKeyColumnIndexes)
        {
            builder.ValidateArgument(nameof(builder));

            return builder.OnDuplicateKeyUpdate(b =>
            {
                var insertColumns = builder.Expressions.FirstOrDefault(x => x.Key == InsertExpressionPositions.Columns);
                if (!insertColumns.Value.HasValue()) throw new InvalidOperationException($"No columns to insert into defined");

                for(int i = 0; i < insertColumns.Value.Length; i++)
                {
                    var column = insertColumns.Value[i].Expression;
                    // Skip primary key columns
                    if(primaryKeyColumnIndexes.Contains(i)) continue;

                    b = b.Set.Expression(column).To.Values(column).And;
                }
            });
        }
        #endregion

        #region Limit
        /// <summary>
        /// Limits the amount of rows returned.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to select</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <param name="limit">Object containing the amount of rows to limit by</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived Limit<TEntity, TDerived>(this ISelectStatementBuilder<TEntity, TDerived> builder, object limit)
        {
            builder.ValidateArgument(nameof(builder));
            limit.ValidateArgument(nameof(limit));

            return Limit(builder, null, limit);
        }
        /// <summary>
        /// Limits the amount of rows returned.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to select</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <param name="offset">Optional offset containing the amount of rows to skip</param>
        /// <param name="limit">Object containing the amount of rows to limit by</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived Limit<TEntity, TDerived>(this ISelectStatementBuilder<TEntity, TDerived> builder, object offset, object limit)
        {
            builder.ValidateArgument(nameof(builder));
            limit.ValidateArgument(nameof(limit));

            var limitExpression = limit is IExpression lE ? lE : new ConstantExpression(limit);
            var offsetExprresion = offset != null ? offset is IExpression oE ? oE : new ConstantExpression(offset) : null;

            var expression = builder.InnerExpressions.FirstOrDefault(x => x is LimitOffsetExpression).CastToOrDefault<LimitOffsetExpression>();

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
                return builder.Expression(new LimitOffsetExpression(limitExpression, offsetExprresion), SelectExpressionPositions.After, 0);
            }
        }
        /// <summary>
        /// Limits the amount of rows updated.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to update</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <param name="limit">Object containing the amount of rows to limit by</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived Limit<TEntity, TDerived>(this IUpdateStatementBuilder<TEntity, TDerived> builder, object limit)
        {
            builder.ValidateArgument(nameof(builder));
            limit.ValidateArgument(nameof(limit));

            return Limit(builder, null, limit);
        }
        /// <summary>
        /// Limits the amount of rows updated.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to update</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <param name="offset">Optional offset containing the amount of rows to skip</param>
        /// <param name="limit">Object containing the amount of rows to limit by</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived Limit<TEntity, TDerived>(this IUpdateStatementBuilder<TEntity, TDerived> builder, object offset, object limit)
        {
            builder.ValidateArgument(nameof(builder));
            limit.ValidateArgument(nameof(limit));

            var limitExpression = limit is IExpression lE ? lE : new ConstantExpression(limit);
            var offsetExprresion = offset != null ? offset is IExpression oE ? oE : new ConstantExpression(offset) : null;

            var expression = builder.InnerExpressions.FirstOrDefault(x => x is LimitOffsetExpression).CastToOrDefault<LimitOffsetExpression>();

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
                return builder.Expression(new LimitOffsetExpression(limitExpression, offsetExprresion), UpdateExpressionPositions.After, 0);
            }
        }
        /// <summary>
        /// Limits the amount of rows deleted.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to delete</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <param name="limit">Object containing the amount of rows to limit by</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived Limit<TEntity, TDerived>(this IDeleteStatementBuilder<TEntity, TDerived> builder, object limit)
        {
            builder.ValidateArgument(nameof(builder));
            limit.ValidateArgument(nameof(limit));

            return Limit(builder, null, limit);
        }
        /// <summary>
        /// Limits the amount of rows deleted.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to delete</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <param name="offset">Optional offset containing the amount of rows to skip</param>
        /// <param name="limit">Object containing the amount of rows to limit by</param>
        /// <returns>Current builder for method chaining</returns>
        public static TDerived Limit<TEntity, TDerived>(this IDeleteStatementBuilder<TEntity, TDerived> builder, object offset, object limit)
        {
            builder.ValidateArgument(nameof(builder));
            limit.ValidateArgument(nameof(limit));

            var limitExpression = limit is IExpression lE ? lE : new ConstantExpression(limit);
            var offsetExprresion = offset != null ? offset is IExpression oE ? oE : new ConstantExpression(offset) : null;

            var expression = builder.InnerExpressions.FirstOrDefault(x => x is LimitOffsetExpression).CastToOrDefault<LimitOffsetExpression>();

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
                return builder.Expression(new LimitOffsetExpression(limitExpression, offsetExprresion), DeleteExpressionPositions.After, 0);
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
        /// Creates a comparison where an expression is compared to the value of a sql parameter concatenated with wildcards. (e.g. CONCAT('%', MyExpression, '%'))
        /// </summary>
        /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
        /// <param name="builder">The builder to add the expressions to</param>
        /// <param name="parameter">The name of the sql parameter</param>
        /// <returns>Builder for creating more expressions</returns>
        public static IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>> LikeParameter<TEntity>(this IStatementConditionOperatorExpressionBuilder<TEntity> builder, string parameter)
        {
            builder.ValidateArgument(nameof(builder));
            parameter.ValidateArgumentNotNullOrWhitespace(nameof(parameter));

            return builder.Like.Concat("%", parameter.AsParameterExpression(), "%");
        }
        #endregion

        #region RecursiveCte
        /// <summary>
        /// Starts to build a cte recursive expression for the current statement.
        /// </summary>
        /// <typeparam name="T">The main entity to map to the cte columns</typeparam>
        /// <param name="builder">The builder to add the cte to</param>
        /// <param name="name">The name of the cte</param>
        /// <returns>Builder for creating the cte expression</returns>
        public static ICteExpressionBuilder<T> RecursiveCte<T>(this ICteStatementBuilder builder , string name)
        {
            builder.ValidateArgument(nameof(builder));
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            var cteExpression = new CteExpression<T>(name);
            var cteBuilder = builder.Expression(new WrapperExpression(new string[] { MySql.Keywords.Recursive, Constants.Strings.Space }, cteExpression, null));
            cteExpression.Builder = cteBuilder;
            return cteExpression;
        }
        /// <summary>
        /// Starts to build a cte recursive expression for the current statement.
        /// </summary>
        /// <typeparam name="T">The main entity to map to the cte columns</typeparam>
        /// <param name="builder">The builder to add the cte to</param>
        /// <returns>Builder for creating the cte expression</returns>
        public static ICteExpressionBuilder<T> RecursiveCte<T>(this ICteStatementBuilder builder) => RecursiveCte<T>(builder, typeof(T).Name);
        /// <summary>
        /// Starts to build a cte recursive expression for the current statement.
        /// </summary>
        /// <param name="builder">The builder to add the cte to</param>
        /// <param name="name">The name of the cte</param>
        /// <returns>Builder for creating the cte expression</returns>
        public static ICteExpressionBuilder<object> RecursiveCte(this ICteStatementBuilder builder, string name) => RecursiveCte<object>(builder, name);
        #endregion

        /// <summary>
        /// Selects the last inserted id.
        /// </summary>
        /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
        /// <typeparam name="TEntity">The main entity to select</typeparam>
        /// <param name="builder">The builder to add the expression to</param>
        /// <returns>Builder for configuring the selected value</returns>
        public static ISelectStatementSelectedValueBuilder<TEntity, TDerived> LastInsertedId<TEntity, TDerived>(this ISelectStatementBuilder<TEntity, TDerived> builder)
        {
            builder.ValidateArgument(nameof(builder));

            return builder.ColumnExpression(b => b.Expression((sb, o) => sb.Append(MySql.Functions.LastInsertId).Append("()")));
        }
    }
}
