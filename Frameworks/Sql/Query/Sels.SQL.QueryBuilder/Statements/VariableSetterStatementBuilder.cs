using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.SQL.QueryBuilder.Statements
{
    /// <inheritdoc cref="IVariableSetterStatementBuilder"/>
    public class VariableSetterStatementBuilder :
        IVariableSetterStatementBuilder,
        IVariableSetterRootStatementBuilder,
        IVariableSetterValueStatementBuilder,
        ISharedExpressionBuilder<object, IVariableSetterStatementBuilder>,
        IEnumerable<IExpression>
    {
        // Fields
        private readonly IStatementCompiler<IVariableSetterStatementBuilder> _compiler;

        /// <inheritdoc cref="VariableSetterStatementBuilder"/>
        /// <param name="compiler">Compiler used to compile the current builder into SQL</param>
        public VariableSetterStatementBuilder(IStatementCompiler<IVariableSetterStatementBuilder> compiler)
        {
            _compiler = compiler.ValidateArgument(nameof(compiler));
        }

        // Properties
        /// <inheritdoc/>
        public IExpression Variable { get; private set; }
        /// <inheritdoc/>
        public IExpression Value { get; private set; }
        /// <inheritdoc/>
        public IExpression[] InnerExpressions => this.ToArray();
        /// <inheritdoc/>
        public ISharedExpressionBuilder<object, IVariableSetterStatementBuilder> To => this;

        /// <inheritdoc/>
        IVariableSetterValueStatementBuilder IVariableSetterRootStatementBuilder.Variable(IExpression expression)
        {
            Variable = expression.ValidateArgument(nameof(expression));
            return this;
        }
        /// <inheritdoc/>
        public IVariableSetterStatementBuilder Expression(IExpression expression)
        {
            Value = expression.ValidateArgument(nameof(expression));
            return this;
        }

        /// <inheritdoc/>
        public StringBuilder Build(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            _compiler.CompileTo(builder, this, null, options);
            return builder;
        }

        /// <inheritdoc/>
        public IEnumerator<IExpression> GetEnumerator()
        {
            if (Variable != null) yield return Variable;
            if (Value != null) yield return Value;
        }
        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
