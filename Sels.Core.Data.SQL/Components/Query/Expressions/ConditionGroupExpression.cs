using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that represents multiple conditions grouped together using ().
    /// </summary>
    public class ConditionGroupExpression : BaseExpressionContainer, IConditionExpression
    {
        // Field
        private List<IExpression> _expressions = new List<IExpression>();

        // Properties
        /// <inheritdoc/>
        public bool IsNot { get; set; }
        /// <inheritdoc/>
        public IExpression[] Expressions => _expressions.ToArray();
        /// <inheritdoc/>
        public LogicOperators? LogicOperator { get; set; }

        /// <inheritdoc/>
        /// <param name="expressions">The expressions to group</param>
        public ConditionGroupExpression(IEnumerable<IExpression> expressions)
        {
            _expressions.AddRange(expressions.ValidateArgumentNotNullOrEmpty(nameof(expressions)));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            if (IsNot) builder.Append(Sql.Not).AppendSpace();

            builder.Append('(');
            _expressions.Execute((i, x) =>
            {
                subBuilder(builder, x);
                if (i != _expressions.Count - 1) builder.AppendSpace();
            });
            builder.Append(')');

            if (LogicOperator.HasValue) builder.AppendSpace().Append(LogicOperator.Value);
        }
    }
}
