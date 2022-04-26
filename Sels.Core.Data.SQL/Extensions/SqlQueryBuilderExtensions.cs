using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL
{
    /// <summary>
    /// Contains extension methods for working with the sql builders.
    /// </summary>
    public static class SqlQueryBuilderExtensions
    {
        /// <summary>
        /// Turns <paramref name="parameter"/> into a <see cref="ParameterExpression"/>.
        /// </summary>
        /// <param name="parameter">The string containing the parameter name</param>
        /// <returns><paramref name="parameter"/> as <see cref="ParameterExpression"/></returns>
        public static ParameterExpression AsParameterExpression(this string parameter)
        {
            parameter.ValidateArgumentNotNullOrWhitespace(nameof(parameter));

            return new ParameterExpression(parameter);
        }
    }
}
