using Sels.Core.Data.SQL.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Attributes.Enumeration.Value;

namespace Sels.Core.Data.SQL.Query.Expressions.Join
{
    /// <summary>
    /// Expression that represents a sql join.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public class JoinExpression<TEntity> : BaseExpressionContainer,
        IOnJoinBuilder<TEntity>,
        IToJoinBuilder<TEntity>,
        IChainedJoinBuilder<TEntity>
    {
        // Fields
        private IExpression? _currentExpression;

        // Properties
        /// <summary>
        /// The type of the join.
        /// </summary>
        public Joins JoinType { get; }
        /// <summary>
        /// The table to join.
        /// </summary>
        public IExpression TableExpression { get; }
        /// <summary>
        /// List of expressions to join on.
        /// </summary>
        public List<(IExpression LeftExpression, IExpression RightExpression)> OnExpressions { get; } = new List<(IExpression LeftExpression, IExpression RightExpression)>();

        /// <inheritdoc cref="JoinExpression{TEntity}"/>
        /// <param name="joinType"><inheritdoc cref="JoinType"/></param>
        /// <param name="tableExpression"><inheritdoc cref="TableExpression"/></param>
        /// <param name="builder">Delegate for configuring the current expression</param>
        public JoinExpression(Joins joinType, IExpression tableExpression, Action<IOnJoinBuilder<TEntity>> builder)
        {
            JoinType = joinType;
            TableExpression = tableExpression.ValidateArgument(nameof(tableExpression));
            builder.ValidateArgument(nameof(builder))(this);
            if (!OnExpressions.HasValue()) throw new InvalidOperationException($"No expressions created using {nameof(builder)}");
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            if(!OnExpressions.HasValue()) throw new InvalidOperationException($"{nameof(OnExpressions)} is empty");

            // Table
            builder.Append(JoinType.GetStringValue()).AppendSpace();
            subBuilder(builder, TableExpression);

            // Columns
            builder.AppendSpace().Append(Sql.On).AppendSpace();

            OnExpressions.Execute((i, e) => {
                var leftExpression = e.LeftExpression ?? throw new InvalidOperationException($"Left expression in on expression <{i}> is not set");
                var rightExpression = e.RightExpression ?? throw new InvalidOperationException($"Right expression in on expression <{i}> is not set");

                subBuilder(builder, leftExpression);
                builder.AppendSpace().Append(Operators.Equal.GetStringValue()).AppendSpace();
                subBuilder(builder, rightExpression);

                if (i != OnExpressions.Count - 1) builder.AppendSpace().Append(LogicOperators.And.GetStringValue()).AppendSpace();
            });
            
        }

        /// <inheritdoc/>
        public IToJoinBuilder<TEntity> OnExpression(IExpression sqlExpression)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));
            if (_currentExpression != null) throw new InvalidOperationException("Expression on left side already defined");

            _currentExpression = sqlExpression;
            return this;
        }
        /// <inheritdoc/>
        public IChainedJoinBuilder<TEntity> ToExpression(IExpression sqlExpression)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));
            if (_currentExpression == null) throw new InvalidOperationException("No expression on the left side defined");

            OnExpressions.Add((_currentExpression, sqlExpression));
            _currentExpression = null;
            return this;
        }
        /// <inheritdoc/>
        public IOnJoinBuilder<TEntity> And()
        {
            return this;
        }

        #region Builder

        #endregion
    }
}
