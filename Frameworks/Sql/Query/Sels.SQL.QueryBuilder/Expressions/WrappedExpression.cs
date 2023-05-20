using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.SQL.QueryBuilder.Expressions
{
    /// <summary>
    /// Sql expression that wraps another expression in ().
    /// </summary>
    public class WrappedExpression : BaseExpressionContainer
    {
        // Fields
        private readonly IExpression _innerExpression;

        /// <inheritdoc cref="WrappedExpression"/>
        /// <param name="innerExpression">The expression to wrap</param>
        public WrappedExpression(IExpression innerExpression)
        {
            _innerExpression = innerExpression.ValidateArgument(nameof(innerExpression));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            builder.Append('(');
            subBuilder(builder, _innerExpression);
            builder.Append(')');
        }
    }
}
