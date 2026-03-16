using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions.Text;

namespace Sels.SQL.QueryBuilder.Expressions.Over
{
    /// <summary>
    /// Expression that sets the lower/upper bound of a window frame to a specific count or unbounded.
    /// </summary>
    public class WindowFrameBorderExpression : BaseExpression
    {
        // Properties
        /// <summary>
        /// The count to limit the bound by. Null means unbounded.
        /// </summary>
        public uint? Count { get; }
        /// <summary>
        /// True when defining the upper bound, otherwise false for the lower bound.
        /// </summary>
        public bool IsUpper { get; }

        /// <inheritdoc cref="WindowFrameBorderExpression"/>
        /// <param name="count"><inheritdoc cref="Count"/></param>
        /// <param name="isUpper"><inheritdoc cref="IsUpper"/></param>
        public WindowFrameBorderExpression(uint? count, bool isUpper)
        {
            Count = count;
            IsUpper = isUpper;
        }

        /// <inheritdoc cref="WindowFrameBorderExpression"/>
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));

            builder.Append(Count.HasValue ? Count.Value.ToString() : Sql.Over.Unbounded);

            builder.AppendSpace().Append(IsUpper ? Sql.Over.Following : Sql.Over.Preceding);
        }
    }
}
