using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Attributes.Enumeration.Value;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that represents a sql function.
    /// </summary>
    public class FunctionExpression : BaseColumnExpression
    {
        /// <summary>
        /// The sql function this expression represents.
        /// </summary>
        public Functions Function { get;}
        /// <summary>
        /// Expression that contains the column to perform the function on.
        /// </summary>
        public IColumnExpression Expression { get; set; }

        /// <inheritdoc cref="FunctionExpression"/>
        /// <param name="function"><inheritdoc cref="Function"/></param>
        /// <param name="expression"><inheritdoc cref="Expression"/></param>
        public FunctionExpression(Functions function, IColumnExpression expression) : base(expression.DataSet, expression.Object, expression.Alias)
        {
            Expression = expression.ValidateArgument(nameof(expression));
            Function = function;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Func<object, string?> datasetConverterer, Func<string, string>? columnConverter = null, bool includeAlias = true, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            datasetConverterer.ValidateArgument(nameof(datasetConverterer));

            var function = Function.GetStringValue();

            builder.Append(function).Append('(');

            Expression.ToSql(builder, datasetConverterer, columnConverter, false);

            builder.Append(')');

            if (includeAlias && Alias != null) builder.AppendSpace().Append(Sql.As).Append(Alias);
        }
    }
}
