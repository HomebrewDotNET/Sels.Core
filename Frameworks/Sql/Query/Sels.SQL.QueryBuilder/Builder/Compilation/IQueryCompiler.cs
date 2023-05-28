using Sels.SQL.QueryBuilder.Builder.Expressions;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Compilation
{
    /// <summary>
    /// Compiler that converts <see cref="IExpression"/> into a query where expression location within the query are defined by <typeparamref name="TPosition"/>. 
    /// </summary>
    /// <typeparam name="TPosition">Type that tells where in the query an expression should be placed</typeparam>
    public interface IQueryCompiler<TPosition>
    {
        /// <summary>
        /// Compiles the expressions in <paramref name="queryBuilder"/> into sql and appends it to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to add the sql to</param>
        /// <param name="queryBuilder">The builder requesting the compilation</param>
        /// <param name="configurator">Optional delegate for configuring the compiler options</param>
        /// <param name="options">Optional settings for building the query</param>
        void CompileTo(StringBuilder builder, IQueryBuilder<TPosition> queryBuilder, Action<ICompilerOptions>? configurator = null, ExpressionCompileOptions options = ExpressionCompileOptions.None);
    }
}
