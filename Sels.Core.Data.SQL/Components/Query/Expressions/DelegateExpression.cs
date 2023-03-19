using System.Text;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that delegates the <see cref="IExpression.ToSql(StringBuilder, ExpressionCompileOptions)"/> call to a delegate.
    /// </summary>
    public class DelegateExpression : BaseExpression, IExpression
    {
        // Fields
        private readonly Action<StringBuilder> _action;

        /// <inheritdoc cref="DelegateExpression"/>
        /// <param name="action">The action to delegate the call to</param>
        public DelegateExpression(Action<StringBuilder> action)
        {
            _action = action.ValidateArgument(nameof(action));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            _action(builder);
        }
    }
}
