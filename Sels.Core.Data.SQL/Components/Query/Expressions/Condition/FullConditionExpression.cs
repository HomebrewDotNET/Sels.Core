using Sels.Core.Data.SQL.Query;
using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions.Condition
{
    /// <summary>
    /// Expression that wraps another <see cref="IExpression"/> so it acts as a condition in a where clause.
    /// </summary>
    public class FullConditionExpression : BaseExpressionContainer, IConditionExpression
    {
        // Properties
        /// <inheritdoc/>
        public bool IsNot { get; set; }
        /// <summary>
        /// The condition expression
        /// </summary>
        public IExpression Expression { get; }
        /// <inheritdoc/>
        public LogicOperators? LogicOperator { get; set; }

        /// <inheritdoc/>
        /// <param name="expression">The condition expression</param>
        public FullConditionExpression(IExpression expression)
        {
            Expression = expression.ValidateArgument(nameof(expression)); 
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            if (IsNot) builder.Append(Sql.Not).AppendSpace();

            subBuilder(builder, Expression);

            if (LogicOperator.HasValue) builder.AppendSpace().Append(LogicOperator.Value);
        }
    }
}
