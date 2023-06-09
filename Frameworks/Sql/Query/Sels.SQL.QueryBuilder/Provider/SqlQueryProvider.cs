using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.Builder.Statement;
using Sels.SQL.QueryBuilder.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Provider
{
    /// <inheritdoc cref="ISqlQueryProvider"/>
    public class SqlQueryProvider : ISqlQueryProvider
    {
        // Fields
        private readonly ISqlCompiler _compiler;
        
        /// <inheritdoc cref="SqlQueryProvider"/>
        /// <param name="compiler">The compiler to use for the statement builders</param>
        public SqlQueryProvider(ISqlCompiler compiler)
        {
            _compiler = compiler.ValidateArgument(nameof(compiler));
        }

        /// <inheritdoc/>
        public IMultiStatementBuilder Build() => new MultiStatementBuilder(_compiler);
        /// <inheritdoc/>
        public IVariableDeclarationRootStatementBuilder Declare() => new VariableDeclarationStatementBuilder(_compiler);
        /// <inheritdoc/>
        public IDeleteStatementBuilder<T> Delete<T>() => new DeleteStatementBuilder<T>(_compiler);
        /// <inheritdoc/>
        public IIfConditionStatementBuilder If() => new IfStatementBuilder(_compiler, _compiler);
        /// <inheritdoc/>
        public IInsertStatementBuilder<T> Insert<T>() => new InsertStatementBuilder<T>(_compiler);
        /// <inheritdoc/>
        public ISelectStatementBuilder<T> Select<T>() => new SelectStatementBuilder<T>(_compiler);
        /// <inheritdoc/>
        public IVariableSetterRootStatementBuilder Set() => new VariableSetterStatementBuilder(_compiler);
        /// <inheritdoc/>
        public IUpdateStatementBuilder<T> Update<T>() => new UpdateStatementBuilder<T>(_compiler);
        /// <inheritdoc/>
        public ICteStatementBuilder With() => new CteStatementBuilder(_compiler);
    }
}
