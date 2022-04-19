using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions.Linq;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that represents a list of constant values in an sql query.
    /// </summary>
    public class ListExpression : BaseExpression, IExpression
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
        public override void ToSql(StringBuilder builder, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));

            builder.Append('(');

            Expressions.Execute((i, e) =>
            {
                e.ToSql(builder, options);
                if (i != Expressions.Length - 1) builder.Append(',');
            });

            builder.Append(')');
        }
    }
}
