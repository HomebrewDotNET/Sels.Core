using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Templates.Query.Expressions.Proxy
{
    /// <summary>
    /// Template that delegates calls from <see cref="IQueryBuilder{TPosition}"/> to an instance.
    /// </summary>
    /// <typeparam name="TPosition">Type that defines where in a query expressions should be located</typeparam>
    public abstract class BaseDelegateQueryBuilder<TPosition> : BaseDelegateQueryBuilder, IQueryBuilder<TPosition>
    {
        // Fields
        private readonly IQueryBuilder<TPosition> _target;

        /// <inheritdoc cref="BaseDelegateQueryBuilder{TPosition}"/>
        /// <param name="target">The instance to delegate calls to</param>
        public BaseDelegateQueryBuilder(IQueryBuilder<TPosition> target) :base(target)
        {
            _target = target.ValidateArgument(nameof(target));
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<TPosition, OrderedExpression[]> Expressions => _target.Expressions;
        /// <inheritdoc/>
        public IQueryBuilder<TPosition> OnExpressionAdded(Action<OrderedExpression, TPosition> action)
        {
            return _target.OnExpressionAdded(action);
        }
    }

    /// <summary>
    /// Template that delegates calls from <see cref="IQueryBuilder"/> to an instance.
    /// </summary>
    public abstract class BaseDelegateQueryBuilder : IQueryBuilder
    {
        // Fields
        private readonly IQueryBuilder _target;

        /// <inheritdoc cref="BaseDelegateQueryBuilder"/>
        /// <param name="target">The instance to delegate calls to</param>
        public BaseDelegateQueryBuilder(IQueryBuilder target)
        {
            _target = target.ValidateArgument(nameof(target));
        }

        /// <inheritdoc/>
        public IExpression[] InnerExpressions => _target.InnerExpressions;
        /// <inheritdoc/>
        public StringBuilder Build(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            return _target.Build(builder, options);
        }
        /// <inheritdoc/>
        public IQueryBuilder OnCompiling(Action<IExpression> action)
        {
            return _target.OnCompiling(action);
        }
        /// <inheritdoc/>
        public IQueryBuilder OnExpressionAdded(Action<IExpression> action)
        {
            return _target.OnExpressionAdded(action);
        }
    }
}
