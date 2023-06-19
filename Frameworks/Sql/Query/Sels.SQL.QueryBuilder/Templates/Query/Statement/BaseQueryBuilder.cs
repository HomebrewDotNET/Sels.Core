using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder
{
    /// <summary>
    /// Base class for creating a <see cref="IQueryBuilder"/>.
    /// </summary>
    public abstract class BaseQueryBuilder : IQueryBuilder
    {
        // Fields
        private List<Action<IExpression>> _expressionHandlers = new List<Action<IExpression>>();
        private List<Action<IExpression>> _expressionCompilationHandlers = new List<Action<IExpression>>();

        /// <inheritdoc/>
        public IQueryBuilder OnExpressionAdded(Action<IExpression> action)
        {
            _expressionHandlers.Add(action.ValidateArgument(nameof(action)));
            return this;
        }
        /// <inheritdoc/>
        public IQueryBuilder OnCompiling(Action<IExpression> action)
        {
            _expressionCompilationHandlers.Add(action.ValidateArgument(nameof(action)));
            return this;
        }

        /// <summary>
        /// Calls all compilation handlers.
        /// </summary>
        /// <param name="expression">The expression being compiled</param>
        protected void OnCompiling(IExpression expression) {
            expression.ValidateArgument(nameof(expression));
            foreach (var handler in _expressionCompilationHandlers)
            {
                handler(expression);
            }
        }

        /// <summary>
        /// Raises an event that <paramref name="expression"/> was added to the current builder.
        /// </summary>
        /// <param name="expression">The expression that was added</param>
        protected void RaiseExpressionAdded(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            foreach (var handler in _expressionHandlers)
            {
                handler(expression);
            }
        }

        /// <inheritdoc/>
        public abstract StringBuilder Build(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None);

        /// <inheritdoc/>
        public abstract IExpression[] InnerExpressions { get; }
    }
}
