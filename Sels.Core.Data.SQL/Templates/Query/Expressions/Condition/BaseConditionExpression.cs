using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions.Condition
{
    /// <summary>
    /// Template for creating a new <see cref="IConditionExpressionBuilder{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    public abstract class BaseConditionExpression<TEntity> : BaseExpressionContainer, IConditionExpressionBuilder<TEntity>, IConditionOperatorExpressionBuilder<TEntity>, IConditionRightExpressionBuilder<TEntity>, IChainedConditionBuilder<TEntity>
    {
        // Fields
        private readonly List<IConditionExpression> _expressions = new List<IConditionExpression>();

        // State
        private bool _nextConditionIsNot = false;
        private ComparisonConditionExpression? _lastExpression;

        // Properties
        /// <summary>
        /// The currently configured condition expressions.
        /// </summary>
        protected IConditionExpression[] Expressions => _expressions.ToArray();

        /// <inheritdoc cref=" BaseConditionExpression{TEntity}"/>
        /// <param name="builder">Delegate for configuring the current builder</param>
        /// <param name="throwOnEmpty">If a <see cref="InvalidOperationException"/> should be thrown when <paramref name="builder"/> created no expressions</param>
        /// <exception cref="InvalidOperationException"></exception>
        public BaseConditionExpression(Action<IConditionExpressionBuilder<TEntity>> builder, bool throwOnEmpty = true)
        {
            builder.ValidateArgument(nameof(builder));
            builder(this);
            if (throwOnEmpty && _expressions.Count == 0) throw new InvalidOperationException($"Expected {nameof(builder)} to create at least 1 expression");
        }

        /// <inheritdoc/>
        public IConditionExpressionBuilder<TEntity> Not()
        {
            _nextConditionIsNot = true;
            return this;
        }
        /// <inheritdoc/>
        public IChainedConditionBuilder<TEntity> WhereGroup(Action<IConditionExpressionBuilder<TEntity>> builder)
        {
            builder.ValidateArgument(nameof(builder));

            return FullExpression(new ConditionGroupExpression<TEntity>(builder, true));
        }
        /// <inheritdoc/>
        public IChainedConditionBuilder<TEntity> FullExpression(IConditionExpression expression)
        {
            expression.ValidateArgument(nameof(expression));

            var isNot = NextConditionIsInverted();
            expression.IsNot = isNot;
            _expressions.Add(expression);
            return this;
        }
        /// <inheritdoc/>
        public IConditionOperatorExpressionBuilder<TEntity> Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            if (_lastExpression != null) throw new InvalidOperationException($"Condition expression already created");

            _lastExpression = new ComparisonConditionExpression(expression);
            return this;
        }
        /// <inheritdoc/>
        public IConditionRightExpressionBuilder<TEntity> CompareTo(IExpression sqlExpression)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));
            if (_lastExpression == null) throw new InvalidOperationException("Expected expression to update but was null");

            _lastExpression.OperatorExpression = sqlExpression;
            return this;
        }
        IChainedConditionBuilder<TEntity> ISharedExpressionBuilder<TEntity, IChainedConditionBuilder<TEntity>>.Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            if (_lastExpression == null) throw new InvalidOperationException("Expected expression to update but was null");
            _lastExpression.RightExpression = expression;

            var builder = FullExpression(_lastExpression);
            _lastExpression = null;
            return builder;
        }
        /// <inheritdoc/>
        public IConditionExpressionBuilder<TEntity> And(LogicOperators logicOperator = LogicOperators.And)
        {
            var last = _expressions.LastOrDefault();
            if (last == null) throw new InvalidOperationException("Expected expression to update but was null");

            last.LogicOperator = logicOperator;
            return this;
        }

        private bool NextConditionIsInverted()
        {
            // Get value from is not and reset
            var isNot = _nextConditionIsNot;
            _nextConditionIsNot = false;
            return isNot;
        }
    }
}
