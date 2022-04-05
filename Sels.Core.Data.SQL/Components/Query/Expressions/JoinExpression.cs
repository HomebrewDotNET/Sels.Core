using Sels.Core.Data.SQL.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Attributes.Enumeration.Value;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that represents a sql join.
    /// </summary>
    public class JoinExpression : BaseExpressionContainer
    {
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

        /// <inheritdoc cref="JoinExpression"/>
        /// <param name="joinType"><inheritdoc cref="JoinType"/></param>
        /// <param name="tableExpression"><inheritdoc cref="TableExpression"/></param>
        public JoinExpression(Joins joinType, IExpression tableExpression)
        {
            JoinType = joinType;
            TableExpression = tableExpression.ValidateArgument(nameof(tableExpression));
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
    }
}
