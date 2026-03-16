using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Expressions.Over
{
    /// <summary>
    /// Expression that sets the lower/upper bound of a window frame to the current row.
    /// </summary>
    public class WindowFrameBorderCurrentRowExpression : BaseExpression
    {
        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder)).Append(Sql.Over.CurrentRow);
        }
    }
}
