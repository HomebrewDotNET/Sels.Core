using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using Sels.SQL.QueryBuilder.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Provider
{
    /// <inheritdoc cref="ISqlQueryProvider"/>
    public class SqlQueryProvider : ISqlQueryProvider, ISqlQueryProviderOptions
    {
        // Fields
        /// <summary>
        /// The compiler to use for the statement builders
        /// </summary>
        protected readonly ISqlCompiler _compiler;
        private readonly List<Action<IQueryBuilder>> _addedHandlers;

        /// <inheritdoc cref="SqlQueryProvider"/>
        /// <param name="compiler">The compiler to use for the statement builders</param>
        public SqlQueryProvider(ISqlCompiler compiler)
        {
            _compiler = compiler.ValidateArgument(nameof(compiler));
        }

        /// <inheritdoc cref="SqlQueryProvider"/>
        /// <param name="compiler">The compiler to use for the statement builders</param>
        /// <param name="configurator">Delegate that configures the current provider</param>
        protected SqlQueryProvider(ISqlCompiler compiler, Action<ISqlQueryProviderOptions> configurator) : this(compiler)
        {
            _addedHandlers = new List<Action<IQueryBuilder>>();
            configurator?.Invoke(this);
        }

        /// <inheritdoc/>
        public IMultiStatementBuilder New() => OnBuilderCreated(new MultiStatementBuilder(_compiler));
        /// <inheritdoc/>
        public IVariableDeclarationRootStatementBuilder Declare() => OnBuilderCreated(new VariableDeclarationStatementBuilder(_compiler));
        /// <inheritdoc/>
        public IDeleteStatementBuilder<T> Delete<T>() => OnBuilderCreated(new DeleteStatementBuilder<T>(_compiler));
        /// <inheritdoc/>
        public IIfConditionStatementBuilder If() => OnBuilderCreated(new IfStatementBuilder(_compiler, _compiler));
        /// <inheritdoc/>
        public IInsertStatementBuilder<T> Insert<T>() => OnBuilderCreated(new InsertStatementBuilder<T>(_compiler));
        /// <inheritdoc/>
        public ISelectStatementBuilder<T> Select<T>() => OnBuilderCreated(new SelectStatementBuilder<T>(_compiler));
        /// <inheritdoc/>
        public IVariableSetterRootStatementBuilder Set() => OnBuilderCreated(new VariableSetterStatementBuilder(_compiler));
        /// <inheritdoc/>
        public IUpdateStatementBuilder<T> Update<T>() => OnBuilderCreated(new UpdateStatementBuilder<T>(_compiler));
        /// <inheritdoc/>
        public ICteStatementBuilder With() => OnBuilderCreated(new CteStatementBuilder(_compiler));

        /// <inheritdoc/>
        public ISqlQueryProvider CreateSubProvider(Action<ISqlQueryProviderOptions> options) => new SqlQueryProvider(_compiler, options.ValidateArgument(nameof(options)));

        /// <inheritdoc/>
        public ISqlQueryProviderOptions OnBuilderCreated(Action<IQueryBuilder> action)
        {
            _addedHandlers.Add(action.ValidateArgument(nameof(action)));
            return this;
        }

        private T OnBuilderCreated<T>(T builder) where T : IQueryBuilder
        {
            builder.ValidateArgument(nameof(builder));

            if (_addedHandlers.HasValue())
            {
                foreach(var handler in _addedHandlers)
                {
                    handler(builder);
                }
            }

            return builder;
        }
    }
}
