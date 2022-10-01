using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Template for creating new <see cref="IExpressionContainer"/> expressions.
    /// </summary>
    public abstract class BaseExpressionContainer : BaseExpression, IExpressionContainer
    {
        /// <inheritdoc/>
        public abstract void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None);
        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            ToSql(builder, (b, e) => e.ToSql(b), options);
        }
    }
}
