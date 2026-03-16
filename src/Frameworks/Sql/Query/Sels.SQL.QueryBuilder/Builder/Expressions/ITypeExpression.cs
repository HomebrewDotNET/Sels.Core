using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{

    /// <summary>
    /// Expression that contains a .net type that can be mapped to an SQL type.
    /// </summary>
    public interface ITypeExpression : IExpression
    {
        /// <summary>
        /// Converts the current expression to sql.
        /// </summary>
        /// <param name="builder">The builder to append to</param>
        /// <param name="typeConverter">Delegate that converts the supplied type and optional length to an SQL type and adds it to the builder</param>
        /// <param name="options">Optional settings for building the query</param>
        public void ToSql(StringBuilder builder, Action<StringBuilder, Type, int?> typeConverter, ExpressionCompileOptions options = ExpressionCompileOptions.None);
    }
}
