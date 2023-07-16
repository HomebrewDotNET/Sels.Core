using Sels.SQL.QueryBuilder.Builder.Statement;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Compilation
{
    /// <summary>
    /// Compiles <typeparamref name="T"/> into SQL.
    /// </summary>
    public interface IStatementCompiler<T>
    {
        /// <summary>
        /// Compiles the expressions in <paramref name="statementBuilder"/> into SQL and appends it to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to add the sql to</param>
        /// <param name="statementBuilder">The builder to compile into sql</param>
        /// <param name="configurator">Optional delegate for configuring the compiler options</param>
        /// <param name="options">Optional settings for building the query</param>
        void CompileTo(StringBuilder builder, T statementBuilder, Action<ICompilerOptions> configurator = null, ExpressionCompileOptions options = ExpressionCompileOptions.None);
    }
}
