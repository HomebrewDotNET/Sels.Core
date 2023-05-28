using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Base builder that wraps an expression that gets compiled into sql using a compiler.
    /// </summary>
    public abstract class BaseExpressionBuilder : IQueryBuilder
    {
        // Fields
        private readonly IExpressionCompiler _compiler;

        /// <inheritdoc cref="BaseExpressionBuilder"/>
        /// <param name="compiler">Compiler to compile the expression into sql</param>
        protected BaseExpressionBuilder(IExpressionCompiler compiler)
        {
            _compiler = compiler.ValidateArgument(nameof(compiler));
        }

        /// <inheritdoc/>
        public string Build(ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            return _compiler.Compile(Expression, null, options);
        }
        /// <inheritdoc/>
        public StringBuilder Build(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            return _compiler.Compile(builder, Expression, null, options);
        }

        /// <inheritdoc/>
        public abstract IExpression[] InnerExpressions { get; }
        /// <summary>
        /// The expression to compile.
        /// </summary>
        protected abstract IExpression Expression { get; }
    }
}
