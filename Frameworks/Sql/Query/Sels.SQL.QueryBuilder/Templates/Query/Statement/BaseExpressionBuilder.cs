using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Base builder that wraps an expression that gets compiled into sql using a compiler.
    /// </summary>
    public abstract class BaseExpressionBuilder : BaseQueryBuilder
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
        public override StringBuilder Build(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            return _compiler.Compile(builder, Expression, x => x.OnCompiling(OnCompiling), options);
        }

        /// <summary>
        /// The expression to compile.
        /// </summary>
        protected abstract IExpression Expression { get; }
    }
}
