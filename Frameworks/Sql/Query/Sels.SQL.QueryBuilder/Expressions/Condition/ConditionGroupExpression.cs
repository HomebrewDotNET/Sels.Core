using System;
using System.Text;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.SQL.QueryBuilder.Builder.Statement;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
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
        /// <param name="throwOnEmpty">If a <see cref="InvalidOperationException"/> should be thrown when <paramref name="builder"/> created no expressions</param>
        public ConditionGroupExpression(Func<IStatementConditionExpressionBuilder<TEntity>, object> builder, bool isGrouped, bool throwOnEmpty = false) : base(builder, throwOnEmpty)
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
