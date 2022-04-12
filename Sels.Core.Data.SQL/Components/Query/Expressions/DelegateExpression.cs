using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that delegates the <see cref="IExpression.ToSql(StringBuilder, QueryBuilderOptions)"/> call to a delegate.
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
        public override void ToSql(StringBuilder builder, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            _action(builder);
        }
    }
}
