using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions;

namespace Sels.SQL.QueryBuilder.Templates.Query.Expressions
{
    /// <summary>
    /// Template for creating a <see cref="ITypeExpression"/>
    /// </summary>
    public abstract class BaseTypeExpression : BaseExpression, ITypeExpression
    {
        /// <inheritdoc/>
        public abstract void ToSql(StringBuilder builder, Action<StringBuilder, Type, int?> typeConverter, ExpressionCompileOptions options = ExpressionCompileOptions.None);

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            throw new NotSupportedException($"No type converter function provided. Sql types differ to much between each dbms to provide a default function");
        }
    }
}
