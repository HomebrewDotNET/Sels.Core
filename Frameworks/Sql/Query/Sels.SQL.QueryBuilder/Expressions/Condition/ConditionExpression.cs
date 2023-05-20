using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.SQL.QueryBuilder.Expressions.Condition
{
    /// <summary>
    /// Expression that represents an Sql condition consisting of 2 expression compared using a logic expression.
    /// </summary>
    public class ConditionExpression : BaseExpressionContainer
    {
        /// <summary>
        /// The expression on the left side of the condition.
        /// </summary>
        public IExpression LeftExpression { get; set; }
        /// <summary>
        /// The expression that compares <see cref="LeftExpression"/> and <see cref="RightExpression"/>
        /// </summary>
        public IExpression OperatorExpression { get; set; }
        /// <summary>
        /// The expression on the right side of the condition.
        /// </summary>
        public IExpression RightExpression { get; set; }
        /// <summary>
        /// Optional expression for chaining additional expressions.
        /// </summary>
        public LogicOperators? LogicOperator { get; set; }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            if (LeftExpression == null) throw new InvalidOperationException($"{nameof(LeftExpression)} is not set");
            if (OperatorExpression == null) throw new InvalidOperationException($"{nameof(OperatorExpression)} is not set");
            if (RightExpression == null) throw new InvalidOperationException($"{nameof(RightExpression)} is not set");

            var expressions = Helper.Collection.Enumerate(LeftExpression, OperatorExpression, RightExpression).ToArray();

            expressions.Execute((i, x) =>
            {
                subBuilder(builder, x);
                if (i != expressions.Length - 1) builder.AppendSpace();
            });

            if (LogicOperator.HasValue) builder.AppendSpace().Append(LogicOperator.Value);
        }
    }
}
