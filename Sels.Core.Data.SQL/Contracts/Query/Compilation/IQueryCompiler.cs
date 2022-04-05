using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Compilation
{
    /// <summary>
    /// Compiler that converts the 
    /// </summary>
    /// <typeparam name="TPosition">Type that tells where in the query an expression should be placed</typeparam>
    public interface IQueryCompiler<TPosition>
    {
        /// <summary>
        /// Compiles <paramref name="expressions"/> into sql and appends it to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to add the sql to</param>
        /// <param name="queryBuilder">The builder requesting the compilation</param>
        /// <param name="expressions">The expressions to compile</param>
        /// <param name="options">Optional settings for building the query</param>
        void CompileTo(StringBuilder builder, IQueryBuilder queryBuilder, IReadOnlyDictionary<TPosition, IExpression[]> expressions, QueryBuilderOptions options = QueryBuilderOptions.None);
    }
}
