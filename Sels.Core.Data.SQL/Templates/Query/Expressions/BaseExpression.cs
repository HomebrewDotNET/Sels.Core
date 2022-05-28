using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Template for creating a <see cref="IExpression"/>.
    /// </summary>
    public abstract class BaseExpression : IExpression
    {
        /// <inheritdoc/>
        public abstract void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None);
        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();
            ToSql(builder, ExpressionCompileOptions.Format);
            return builder.ToString();
        }
    }
}
