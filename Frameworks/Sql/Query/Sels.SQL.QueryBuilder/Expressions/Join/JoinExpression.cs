using System.Text;
using Sels.Core.Attributes.Enumeration.Value;
using Sels.SQL.QueryBuilder.Builder.Statement;
using Sels.SQL.QueryBuilder.Expressions.Condition;

namespace Sels.SQL.QueryBuilder.Builder.Expressions.Join
{
    /// <summary>
    /// Expression that represents a sql join.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TDerived">The builder that created the expression</typeparam>
    public class JoinExpression<TEntity, TDerived> : BaseExpressionContainer,
        IStatementJoinTableBuilder<TEntity, TDerived>,
        IStatementJoinOnBuilder<TEntity, TDerived>,
        IStatementJoinConditionBuilder<TEntity>,
        IComparisonExpressionBuilder<TEntity, IStatementJoinFinalConditionBuilder<TEntity>>,
        IStatementJoinFinalConditionBuilder<TEntity>,
        IChainedBuilder<TEntity, IStatementJoinConditionBuilder<TEntity>>
    {
        // Fields
        private readonly TDerived _derived;
        private readonly List<ConditionExpression> _conditions = new List<ConditionExpression>();

        // Properties
        /// <summary>
        /// The type of the join.
        /// </summary>
        public Joins JoinType { get; }
        /// <summary>
        /// The table to join.
        /// </summary>
        public TableExpression TableExpression { get; private set; }
        /// <summary>
        /// Array of expressions to join on.
        /// </summary>
        public IExpressionContainer[] OnExpressions => _conditions.ToArray();

        /// <inheritdoc cref="JoinExpression{TEntity, TDerived}"/>
        /// <param name="joinType"><inheritdoc cref="JoinType"/></param>
        /// <param name="parent">The builder that created this expression so it can be returned later when done building this expression</param>
        public JoinExpression(Joins joinType, TDerived parent)
        {
            JoinType = joinType;
            _derived = parent.ValidateArgument(nameof(parent));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            var expression = OnExpressions;
            if(!expression.HasValue()) throw new InvalidOperationException($"{nameof(OnExpressions)} is empty");

            // Table
            builder.Append(JoinType.GetStringValue()).AppendSpace();
            subBuilder(builder, TableExpression);

            // Columns
            builder.AppendSpace().Append(Sql.On).AppendSpace();

            expression.Execute((i, e) => {
                subBuilder(builder, e);

                if (i != expression.Length - 1) builder.AppendSpace();
            });
            
        }

        /// <inheritdoc/>
        public IStatementJoinOnBuilder<TEntity, TDerived> Table(string table, object? datasetAlias = null, string? database = null, string? schema = null)
        {
            table.ValidateArgument(nameof(table));

            TableExpression = new TableExpression(database, schema, table, datasetAlias);

            return this;
        }
        /// <inheritdoc/>
        public TDerived On(Action<IStatementJoinConditionBuilder<TEntity>> builder)
        {
            builder.ValidateArgument(nameof(builder));

            builder(this);

            if (!_conditions.HasValue()) throw new InvalidOperationException($"No expressions created using {nameof(builder)}");
            return _derived;
        }
        /// <inheritdoc/>
        public IComparisonExpressionBuilder<TEntity, IStatementJoinFinalConditionBuilder<TEntity>> Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));

            _conditions.Add(new ConditionExpression() { LeftExpression = expression });

            return this;
        }
        /// <inheritdoc/>
        public IStatementJoinFinalConditionBuilder<TEntity> CompareTo(IExpression sqlExpression)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));

            _conditions.Last().OperatorExpression = sqlExpression;

            return this;
        }
        /// <inheritdoc/>
        IChainedBuilder<TEntity, IStatementJoinConditionBuilder<TEntity>> ISharedExpressionBuilder<TEntity, IChainedBuilder<TEntity, IStatementJoinConditionBuilder<TEntity>>>.Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));

            _conditions.Last().RightExpression = expression;

            return this;
        }
        /// <inheritdoc/>
        public IStatementJoinConditionBuilder<TEntity> AndOr(LogicOperators logicOperator = LogicOperators.And)
        {
            _conditions.Last().LogicOperator = logicOperator;

            return this;
        }
    }
}
