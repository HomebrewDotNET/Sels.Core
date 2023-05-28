﻿using Sels.Core.Models;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Expressions.Update;
using Sels.SQL.QueryBuilder.Builder.Statement;
using Sels.SQL.QueryBuilder.Expressions.Condition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sels.SQL.QueryBuilder.Expressions
{
    /// <summary>
    /// Expression that represents a CASE Sql expression.
    /// </summary>
    ///  <typeparam name="TEntity">The main entity to create the expression for</typeparam>
    public class CaseExpression<TEntity> : BaseExpressionContainer, 
        ICaseExpressionRootBuilder<TEntity>,
        ICaseExpressionConditionBuilder<TEntity>,
        IComparisonExpressionBuilder<TEntity, ICaseExpressionFinalConditionBuilder<TEntity>>,
        ICaseExpressionFinalConditionBuilder<TEntity>,
        IChainedBuilder<TEntity, ICaseExpressionConditionBuilder<TEntity>>,
        ICaseExpressionThenBuilder<TEntity>,
        ISharedExpressionBuilder<TEntity, ICaseExpressionBuilder<TEntity>>,
        ICaseExpressionBuilder<TEntity>,
        ISharedExpressionBuilder<TEntity, Null>
    {
        // Fields
        private IExpression? _currentWhenExpression;
        private List<(IExpression Condition, IExpression Value)> _whenExpressions = new List<(IExpression Condition, IExpression Value)>();
        private IExpression _elseExpression;

        // Properties
        /// <inheritdoc/>
        public ISharedExpressionBuilder<TEntity, ICaseExpressionBuilder<TEntity>> Then => this;
        /// <inheritdoc/>
        public ISharedExpressionBuilder<TEntity, Null> Else => this;

        /// <inheritdoc cref="CaseExpression{TEntity}"/>
        public CaseExpression()
        {
                
        }
        /// <inheritdoc cref="CaseExpression{TEntity}"/>
        /// <param name="configurator">Delegate for configuring the current instance</param>
        public CaseExpression(Action<ICaseExpressionRootBuilder<TEntity>> configurator)
        {
            configurator.ValidateArgument(nameof(configurator)).Invoke(this);      
        }
        /// <inheritdoc/>
        public ICaseExpressionThenBuilder<TEntity> When(Action<ICaseExpressionConditionBuilder<TEntity>> whenBuilder)
        {
            whenBuilder.ValidateArgument(nameof(whenBuilder));
            if (_currentWhenExpression != null) throw new InvalidOperationException($"When expression already defined");

            var conditionContainer = new ExpressionContainer();
            conditionContainer.InnerExpressions.Add(new ConditionExpression());
            _currentWhenExpression = conditionContainer;
            whenBuilder(this);
            return this;
        }
        /// <inheritdoc/>
        public ICaseExpressionThenBuilder<TEntity> When(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            if (_currentWhenExpression != null) throw new InvalidOperationException($"When expression already defined");

            _currentWhenExpression = expression;
            return this;
        }
        /// <inheritdoc/>
        IComparisonExpressionBuilder<TEntity, ICaseExpressionFinalConditionBuilder<TEntity>> ISharedExpressionBuilder<TEntity, IComparisonExpressionBuilder<TEntity, ICaseExpressionFinalConditionBuilder<TEntity>>>.Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            if(_currentWhenExpression is ExpressionContainer container)
            {
                if(container.InnerExpressions.LastOrDefault() is ConditionExpression conditionExpression)
                {
                    conditionExpression.LeftExpression = expression;
                    return this;
                }
            }

            throw new InvalidOperationException($"Expected last expression to be <{typeof(ConditionExpression)}>");
        }
        /// <inheritdoc/>
        public ICaseExpressionFinalConditionBuilder<TEntity> CompareTo(IExpression sqlExpression)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));
            if (_currentWhenExpression is ExpressionContainer container)
            {
                if (container.InnerExpressions.LastOrDefault() is ConditionExpression conditionExpression)
                {
                    conditionExpression.OperatorExpression = sqlExpression;
                    return this;
                }
            }

            throw new InvalidOperationException($"Expected last expression to be <{typeof(ConditionExpression)}>");
        }
        /// <inheritdoc/>
        IChainedBuilder<TEntity, ICaseExpressionConditionBuilder<TEntity>> ISharedExpressionBuilder<TEntity, IChainedBuilder<TEntity, ICaseExpressionConditionBuilder<TEntity>>>.Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            if (_currentWhenExpression is ExpressionContainer container)
            {
                if (container.InnerExpressions.LastOrDefault() is ConditionExpression conditionExpression)
                {
                    conditionExpression.RightExpression = expression;
                    return this;
                }
            }

            throw new InvalidOperationException($"Expected last expression to be <{typeof(ConditionExpression)}>");
        }
        /// <inheritdoc/>
        public ICaseExpressionConditionBuilder<TEntity> AndOr(LogicOperators logicOperator = LogicOperators.And)
        {
            if (_currentWhenExpression is ExpressionContainer container)
            {
                if (container.InnerExpressions.LastOrDefault() is ConditionExpression conditionExpression)
                {
                    conditionExpression.LogicOperator = logicOperator;
                    return this;
                }
            }

            throw new InvalidOperationException($"Expected last expression to be <{typeof(ConditionExpression)}>");
        }

        /// <inheritdoc/>
        public ICaseExpressionThenBuilder<TEntity> Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            if (_currentWhenExpression != null) throw new InvalidOperationException($"When expression was already set");

            _currentWhenExpression = expression;
            return this;
        }
        /// <inheritdoc/>
        ICaseExpressionBuilder<TEntity> ISharedExpressionBuilder<TEntity, ICaseExpressionBuilder<TEntity>>.Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            if (_currentWhenExpression == null) throw new InvalidOperationException($"When expression is not set");

            _whenExpressions.Add((_currentWhenExpression, expression));
            _currentWhenExpression = null;
            return this;
        }
        /// <inheritdoc/>
        Null ISharedExpressionBuilder<TEntity, Null>.Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            _elseExpression = expression;
            return Null.Value;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));
            if (!_whenExpressions.HasValue()) throw new InvalidOperationException($"No WHEN THEN expression defined");

            // CASE
            builder.Append(Sql.Case);

            // WHEN THEN
            foreach(var (conditionExpression, valueExpression) in _whenExpressions)
            {
                builder.AppendSpace()
                       .Append(Sql.When)
                       .AppendSpace();
                subBuilder(builder, conditionExpression);
                builder.AppendSpace()
                       .Append(Sql.Then)
                       .AppendSpace();
                subBuilder(builder, valueExpression);
            }

            // ELSE
            if(_elseExpression != null)
            {
                builder.Append(Sql.Else)
                       .AppendSpace();
                subBuilder(builder, _elseExpression);
            }

            // END
            builder.AppendSpace()
                   .Append(Sql.End);
        }
    }
}