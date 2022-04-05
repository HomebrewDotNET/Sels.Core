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
    public abstract class BaseExpressionContainer : IExpressionContainer
    {
        /// <inheritdoc/>
        public abstract void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, QueryBuilderOptions options = QueryBuilderOptions.None);
        /// <inheritdoc/>
        public void ToSql(StringBuilder builder, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            ToSql(builder, (b, e) => e.ToSql(b), options);
        }
    }
}
