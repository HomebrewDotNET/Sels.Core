﻿using Sels.Core.Extensions;
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
    /// <inheritdoc cref="IVariableDeclarationStatementBuilder"/>
    public class VariableDeclarationStatementBuilder : BaseQueryBuilder,
        IVariableDeclarationRootStatementBuilder, 
        IVariableDeclarationTypeStatementBuilder, 
        IVariableDeclarationStatementBuilder,
        ISharedExpressionBuilder<object, IVariableDeclarationStatementBuilder>,
        IEnumerable<IExpression>
    {
        // Fields
        private readonly IStatementCompiler<IVariableDeclarationStatementBuilder> _compiler;

        // Properties
        /// <inheritdoc/>
        public IExpression Variable { get; private set; }
        /// <inheritdoc/>
        public IExpression Type { get; private set; }
        /// <inheritdoc/>
        public IExpression InitialValue { get; private set; }
        /// <inheritdoc/>
        public ISharedExpressionBuilder<object, IVariableDeclarationStatementBuilder> InitialzedBy => this;
        /// <inheritdoc/>
        public override IExpression[] InnerExpressions => this.ToArray();

        /// <inheritdoc cref="VariableDeclarationStatementBuilder"/>
        /// <param name="compiler">Compiler used to compile the current builder into SQL</param>
        public VariableDeclarationStatementBuilder(IStatementCompiler<IVariableDeclarationStatementBuilder> compiler)
        {
            _compiler = compiler.ValidateArgument(nameof(compiler));
        }
        /// <inheritdoc/>
        IVariableDeclarationTypeStatementBuilder IVariableDeclarationRootStatementBuilder.Variable(IExpression expression)
        {
            Variable = expression.ValidateArgument(nameof(expression));
            RaiseExpressionAdded(expression);
            return this;
        }
        /// <inheritdoc/>
        public IVariableDeclarationStatementBuilder As(IExpression expression)
        {
            Type = expression.ValidateArgument(nameof(expression));
            RaiseExpressionAdded(expression);
            return this;
        }
        /// <inheritdoc/>
        public IVariableDeclarationStatementBuilder Expression(IExpression expression)
        {
            InitialValue = expression.ValidateArgument(nameof(expression));
            RaiseExpressionAdded(expression);
            return this;
        }

        /// <inheritdoc/>
        public override StringBuilder Build(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            _compiler.CompileTo(builder, this, null, options);
            return builder;
        }

        /// <inheritdoc/>
        public IEnumerator<IExpression> GetEnumerator()
        {
            if (Variable != null) yield return Variable;
            if (Type != null) yield return Type;
            if (InitialValue != null) yield return InitialValue;
        }
        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
