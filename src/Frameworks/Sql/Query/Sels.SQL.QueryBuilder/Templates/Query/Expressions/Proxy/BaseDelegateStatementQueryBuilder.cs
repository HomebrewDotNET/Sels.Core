using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Templates.Query.Expressions.Proxy
{
    /// <summary>
    /// Delegates calls from <see cref="IStatementQueryBuilder{TEntity, TPosition, TDerived}"/> to an instance.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    /// <typeparam name="TPosition">Type that defines where in a query expressions should be located</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public abstract class BaseDelegateStatementQueryBuilder<TEntity, TPosition, TDerived> : BaseDelegateQueryBuilder<TPosition>, IStatementQueryBuilder<TEntity, TPosition, TDerived>
    {
        // Fields
        private readonly IStatementQueryBuilder<TEntity, TPosition, TDerived> _target;

        /// <inheritdoc cref="BaseDelegateStatementQueryBuilder{TEntity, TPosition, TDerived}"/>
        /// <param name="target">The instance to delegate calls to</param>
        protected BaseDelegateStatementQueryBuilder(IStatementQueryBuilder<TEntity, TPosition, TDerived> target) : base(target)
        {
            _target = target.ValidateArgument(nameof(target));
        }

        /// <inheritdoc/>
        public TDerived Instance => _target.Instance;
        /// <inheritdoc/>
        public IReadOnlyDictionary<Type, string> Aliases => _target.Aliases;
        /// <inheritdoc/>
        public TDerived AliasFor<T>(string tableAlias)
        {
            return _target.AliasFor<T>(tableAlias);
        }
        /// <inheritdoc/>
        public TDerived Clone()
        {
            return _target.Clone();
        }
        /// <inheritdoc/>
        public TDerived Expression(IExpression sqlExpression, TPosition position, int order = 0)
        {
            return _target.Expression(sqlExpression, position, order);
        }
        /// <inheritdoc/>
        public string GetAlias<T>()
        {
            return _target.GetAlias<T>();
        }
        /// <inheritdoc/>
        public string GetAlias(Type type)
        {
            return _target.GetAlias(type);
        }
        /// <inheritdoc/>
        public TDerived OutAlias<T>(out string tableAlias)
        {
            return _target.OutAlias<T>(out tableAlias);
        }
        /// <inheritdoc/>
        public void SetAlias(Type type, string alias)
        {
            _target.SetAlias(type, alias);
        }
    }
}
