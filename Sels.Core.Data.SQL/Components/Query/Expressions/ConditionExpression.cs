using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that represents a where clause where 2 expression are compared using an operator expression.
    /// </summary>
    public class ConditionExpression : BaseExpressionContainer, IConditionExpression
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
        public IExpression[] Expressions => Helper.Collection.Enumerate(LeftExpression, OperatorExpression, RightExpression).ToArray();

        /// <inheritdoc/>
        /// <param name="expression"><inheritdoc cref="LeftExpression"/></param>
        public ConditionExpression(IExpression expression)
        {
            LeftExpression = expression.ValidateArgument(nameof(expression));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            if (OperatorExpression == null) throw new InvalidOperationException($"{nameof(OperatorExpression)} is not set");
            if (RightExpression == null) throw new InvalidOperationException($"{nameof(RightExpression)} is not set");

            var expressions = Expressions;
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
