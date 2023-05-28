using Sels.SQL.QueryBuilder.Builder.Expressions;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Compilation
{
    /// <summary>
    /// Compiler that converts <see cref="IExpression"/> into sql.
    /// </summary>
    public interface IExpressionCompiler
    {
        /// <summary>
        /// Compiles <paramref name="expression"/> into a sql string.
        /// </summary>
        /// <param name="expression">The expression to compile into sql</param>
        /// <param name="configurator">Optional delegate for configuring the compiler options</param>
        /// <param name="options"><inheritdoc cref="ExpressionCompileOptions"/></param>
        /// <returns><paramref name="expression"/> compiled into an sql string</returns>
        string Compile(IExpression expression, Action<ICompilerOptions>? configurator = null, ExpressionCompileOptions options = ExpressionCompileOptions.None);
        /// <summary>
        /// Compiler <paramref name="expression"/> into a sql string and appends it to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to add the sql string to</param>
        /// <param name="configurator">Optional delegate for configuring the compiler options</param>
        /// <param name="expression">The expression to compile into sql</param>
        /// <param name="options"><inheritdoc cref="ExpressionCompileOptions"/></param>
        /// <returns><paramref name="builder"/> for method chaining</returns>
        StringBuilder Compile(StringBuilder builder, IExpression expression, Action<ICompilerOptions>? configurator = null, ExpressionCompileOptions options = ExpressionCompileOptions.None);
    }
}
