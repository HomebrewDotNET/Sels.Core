using Sels.Core;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using System;
using System.Linq;
using System.Text;
using Sels.Core.Extensions.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that represents a condition where 2 expressions are compared using a operator expression
    /// </summary>
    public class ComparisonConditionExpression : BaseExpressionContainer, IConditionExpression
    {
        // Properties
        /// <inheritdoc/>
        public bool IsNot { get; set; }
        /// <summary>
        /// Expression on the left side of the condition.
        /// </summary>
        public IExpression LeftExpression { get; }
        /// <summary>
        /// Expression containing the comparator for the condition.
        /// </summary>
        public IExpression OperatorExpression { get; set; }
        /// <summary>
        /// Expression on the right side of the condition.
        /// </summary>
        public IExpression RightExpression { get; set; }
        /// <inheritdoc/>
        public LogicOperators? LogicOperator { get; set; }

        /// <inheritdoc/>
        /// <param name="expression"><inheritdoc cref="LeftExpression"/></param>
        public ComparisonConditionExpression(IExpression expression)
        {
            LeftExpression = expression.ValidateArgument(nameof(expression));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            if (OperatorExpression == null) throw new InvalidOperationException($"{nameof(OperatorExpression)} is not set");
            if (RightExpression == null) throw new InvalidOperationException($"{nameof(RightExpression)} is not set");

            var expressions = Helper.Collection.Enumerate(LeftExpression, OperatorExpression, RightExpression).ToArray();
            if (IsNot) builder.Append(Sql.Not).AppendSpace();

            expressions.Execute((i, x) =>
            {
                subBuilder(builder, x);
                if (i != expressions.Length - 1) builder.AppendSpace();
            });

            if(LogicOperator.HasValue) builder.AppendSpace().Append(LogicOperator.Value);
        }
    }
}
