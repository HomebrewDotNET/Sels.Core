using Sels.Core.Data.SQL.Query.Expressions;
using Sels.Core.Data.SQL.Query.Statement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Compilation
{
    /// <summary>
    /// Compiler that converts <see cref="IExpression"/> into a query where expressions locations within the query are defined by <typeparamref name="TPosition"/>. 
    /// </summary>
    /// <typeparam name="TPosition">Type that tells where in the query an expression should be placed</typeparam>
    public interface IQueryCompiler<TPosition>
    {
        /// <summary>
        /// Compiles the expressions in <paramref name="queryBuilder"/> into sql and appends it to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to add the sql to</param>
        /// <param name="queryBuilder">The builder requesting the compilation</param>
        /// <param name="datasetConverterer">Optional dataset converter for converting dataset objects into a string</param>
        /// <param name="options">Optional settings for building the query</param>
        void CompileTo(StringBuilder builder, IQueryBuilder<TPosition> queryBuilder, Func<object, string?>? datasetConverterer = null, ExpressionCompileOptions options = ExpressionCompileOptions.None);
    }
}
