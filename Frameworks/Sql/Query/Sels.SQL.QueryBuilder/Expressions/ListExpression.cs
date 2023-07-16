using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that represents a list of constant values in an sql query.
    /// </summary>
    public class ListExpression : BaseExpressionContainer, IExpression
    {
        /// <summary>
        /// Expressions that form the list of constant values.
        /// </summary>
        public IExpression[] Expressions { get; }

        /// <inheritdoc cref="ListExpression"/>
        /// <param name="expressions"><inheritdoc cref="Expressions"/></param>
        public ListExpression(IEnumerable<IExpression> expressions)
        {
            Expressions = expressions.ValidateArgumentNotNullOrEmpty(nameof(expressions)).ToArray();
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));
            builder.Append('(');

            Expressions.Execute((i, e) =>
            {
                subBuilder(builder, e);
                if (i != Expressions.Length - 1) builder.Append(',');
            });

            builder.Append(')');
        }
    }
}
