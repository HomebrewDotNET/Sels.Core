using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that contains sub expressions that can be converted to sql.
    /// </summary>
    public interface IExpressionContainer : IExpression
    {
        /// <summary>
        /// Converts the current expression to sql.
        /// </summary>
        /// <param name="builder">The builder to append to</param>
        /// <param name="subBuilder">Delegate for appending any sub expression to <paramref name="builder"/></param>
        /// <param name="options">Optional settings for building the query</param>
        public void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None);
    }
}
