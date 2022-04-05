using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that converts the supplied object to a sql string using <see cref="object.ToString()"/>.
    /// </summary>
    public class RawExpression : IExpression
    {
        /// <summary>
        /// The object containing the sql expression.
        /// </summary>
        public object Expression { get; }

        /// <inheritdoc cref="RawExpression"/>
        /// <param name="expression"><inheritdoc cref="Expression"/></param>
        public RawExpression(object expression)
        {
            Expression = expression.ValidateArgument(nameof(expression));
        }

        /// <inheritdoc/>
        public void ToSql(StringBuilder builder, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            builder.Append(Expression);
        }
    }
}
