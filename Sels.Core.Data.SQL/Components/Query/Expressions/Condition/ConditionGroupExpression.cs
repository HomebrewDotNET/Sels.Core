using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Data.SQL.Query.Statement;

namespace Sels.Core.Data.SQL.Query.Expressions.Condition
{
    /// <summary>
    /// Expression that represents multiple conditions.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the condition group for</typeparam>
    public class ConditionGroupExpression<TEntity> : BaseConditionExpression<TEntity>, IConditionExpression
    {
        // Fields
        private readonly bool _isGrouped;

        // Properties
        /// <inheritdoc/>
        public bool IsNot { get; set; }
        /// <inheritdoc/>
        public LogicOperators? LogicOperator { get; set; }

        /// <inheritdoc/>
        /// <param name="builder">Delegate for configuring the current condition group</param>
        /// <param name="isGrouped">If the condition in this expression should be grouped using ()</param>
        public ConditionGroupExpression(Action<IStatementConditionExpressionBuilder<TEntity>> builder, bool isGrouped) : base(builder, true)
        {
            _isGrouped = isGrouped;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            if (IsNot) builder.Append(Sql.Not).AppendSpace();
            var expressions = Expressions;

            if(_isGrouped) builder.Append('(');
            expressions.Execute((i, x) =>
            {
                if (x is NullExpression) return;
                subBuilder(builder, x);
                if (i != expressions.Length - 1) builder.AppendSpace();
            });
            if (_isGrouped) builder.Append(')');

            if (LogicOperator.HasValue) builder.AppendSpace().Append(LogicOperator.Value);
        }
    }
}
