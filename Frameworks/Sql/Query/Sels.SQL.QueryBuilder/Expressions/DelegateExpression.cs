using Sels.Core.Extensions;
using System;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that delegates the <see cref="IExpression.ToSql(StringBuilder, ExpressionCompileOptions)"/> call to a delegate.
    /// </summary>
    public class DelegateExpression : BaseExpression, IExpression
    {
        // Fields
        private readonly Action<StringBuilder, ExpressionCompileOptions> _action;

        /// <inheritdoc cref="DelegateExpression"/>
        /// <param name="action">The action to delegate the call to</param>
        public DelegateExpression(Action<StringBuilder, ExpressionCompileOptions> action)
        {
            _action = action.ValidateArgument(nameof(action));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            _action(builder, options);
        }
    }
}
