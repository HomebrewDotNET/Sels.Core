using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query
{
    /// <summary>
    /// Template for creating a <see cref="IQueryBuilder"/>.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    public abstract class BaseQueryBuilder<TEntity, TDerived> : IQueryBuilder<TEntity, TDerived>, IQueryJoinBuilder<TEntity, TDerived>, IOnJoinBuilder<TEntity, TDerived>, IToJoinBuilder<TEntity, TDerived>, IChainedJoinBuilder<TEntity, TDerived>, IConditionBuilder<TEntity, TDerived>, IChainedConditionBuilder<TEntity, TDerived>, IConditionOperatorBuilder<TEntity, TDerived>, IConditionRightExpressionBuilder<TEntity, TDerived>
    {
        // Fields
        private readonly Dictionary<Type, string> _aliases = new Dictionary<Type, string>();

        // State
        private JoinExpression? _lastJoinExpression;

        private IConditionExpression? _lastConditionExpression;
        private ConditionExpression? _lastTypedConditionExpression;
        private Action<ConditionExpression> _conditionSetter;
        private bool _nextConditionIsNot = false;

        // Properties
        /// <inheritdoc/>
        public IReadOnlyDictionary<Type, string> Aliases => _aliases;

        #region Alias
        /// <inheritdoc/>
        public TDerived AliasFor<T>(string tableAlias)
        {
            tableAlias.ValidateArgumentNotNullOrWhitespace(nameof(tableAlias));

            _aliases.AddOrUpdate(typeof(T), tableAlias);
            return Instance;
        }
        /// <inheritdoc/>
        public string GetAlias<T>()
        {
            return GetAlias(typeof(T));
        }
        /// <inheritdoc/>
        public string GetAlias(Type type)
        {
            type.ValidateArgument(nameof(type));

            return GetOrSetAlias(type);
        }
        /// <inheritdoc/>
        public TDerived OutAlias<T>(out string tableAlias)
        {
            tableAlias = GetAlias<T>();
            return Instance;
        }

        private string GetOrSetAlias(Type type)
        {
            type.ValidateArgument(nameof(type));

            if (_aliases.ContainsKey(type))
            {
                return _aliases[type];
            }
            else
            {
                var alias = type.Name[0].ToString();

                var counter = 1;
                while(_aliases.Any(x => x.Value.Equals(alias, StringComparison.OrdinalIgnoreCase)))
                {
                    alias = alias + counter;
                    counter++;
                }

                _aliases.Add(type, alias);
                return alias;
            }
        }
        #endregion

        #region Join
        /// <inheritdoc/>
        public IOnJoinBuilder<TEntity, TDerived> JoinExpression(Joins joinType, IExpression sqlExpression)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));

            _lastJoinExpression = new JoinExpression(joinType, sqlExpression);
            AddJoinExpression(_lastJoinExpression);
            return this;
        }
        /// <inheritdoc/>
        public IToJoinBuilder<TEntity, TDerived> OnExpression(IExpression sqlExpression)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));
            if (_lastJoinExpression == null) throw new InvalidOperationException("No join expression was created");
            _lastJoinExpression.OnExpressions.Add((sqlExpression, null));
            return this;
        }
        /// <inheritdoc/>
        public IChainedJoinBuilder<TEntity, TDerived> ToExpression(IExpression sqlExpression)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));
            if (_lastJoinExpression == null) throw new InvalidOperationException("No join expression was created");
            var last = _lastJoinExpression.OnExpressions.Last();
            _lastJoinExpression.OnExpressions.RemoveAt(_lastJoinExpression.OnExpressions.Count-1);
            _lastJoinExpression.OnExpressions.Add((last.LeftExpression, sqlExpression));
            return this;
        }
        /// <inheritdoc/>
        public IOnJoinBuilder<TEntity, TDerived> And()
        {
            return this;
        }
        /// <inheritdoc/>
        public IQueryJoinBuilder<TEntity, TDerived> Also()
        {
            _lastJoinExpression = null;
            return this;
        }
        /// <inheritdoc/>
        TDerived IChainedJoinBuilder<TEntity, TDerived>.Exit()
        {
            return Instance;
        }
        #endregion

        #region Condition
        /// <inheritdoc/>
        public IConditionBuilder<TEntity, TDerived> Not()
        {
            _nextConditionIsNot = true;
            return this;
        }
        /// <inheritdoc/>
        public IChainedConditionBuilder<TEntity, TDerived> WhereGroup(Action<IConditionBuilder<TEntity, TDerived>> builder)
        {
            builder.ValidateArgument(nameof(builder));
            var expressions = new List<IConditionExpression>();
            var isNot = NextConditionIsInverted();

            // Intercept conditions
            var oldSetter = _conditionSetter;
            _conditionSetter = x => expressions.Add(x);

            // Create conditions
            builder(this);

            // Reset old setter
            _conditionSetter = oldSetter;

            AddCondition(new ConditionGroupExpression(expressions) { IsNot = isNot });
            return this;
        }
        /// <inheritdoc/>
        public IConditionOperatorBuilder<TEntity, TDerived> WhereExpression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            var isNot = NextConditionIsInverted();

            AddCondition(new ConditionExpression(expression) { IsNot = isNot });
            return this;
        }
        /// <inheritdoc/>
        public IConditionRightExpressionBuilder<TEntity, TDerived> CompareTo(IExpression sqlExpression)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));
            if (_lastTypedConditionExpression == null) throw new InvalidOperationException("Expected expression to set but was null");
            _lastTypedConditionExpression.OperatorExpression = sqlExpression;
            return this;
        }
        /// <inheritdoc/>
        public IChainedConditionBuilder<TEntity, TDerived> Expression(IExpression sqlExpression)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));
            if (_lastTypedConditionExpression == null) throw new InvalidOperationException("Expected expression to set but was null");
            _lastTypedConditionExpression.RightExpression = sqlExpression;
            return this;
        }
        /// <inheritdoc/>
        public IConditionBuilder<TEntity, TDerived> And(LogicOperators logicOperator = LogicOperators.And)
        {
            if (_lastConditionExpression == null) throw new InvalidOperationException("Expected expression to set but was null");

            _lastConditionExpression.LogicOperator = logicOperator;
            _lastConditionExpression = null;
            _lastTypedConditionExpression = null;
            return this;
        }
        /// <inheritdoc/>
        TDerived IChainedConditionBuilder<TEntity, TDerived>.Exit()
        {
            _lastConditionExpression = null;
            _lastTypedConditionExpression = null;
            return Instance;
        }

        private bool NextConditionIsInverted()
        {
            // Get value from is not and reset
            var isNot = _nextConditionIsNot;
            _nextConditionIsNot = false;
            return isNot;
        }
        private void AddCondition(IConditionExpression expression)
        {
            _lastConditionExpression = expression;

            if(expression is ConditionExpression conditionExpression)
            {
                _lastTypedConditionExpression = conditionExpression;

                if(_conditionSetter != null)
                {
                    _conditionSetter(conditionExpression);
                    return;
                }
            }

            AddConditionExpression(expression);
        }
        #endregion

        // Abstractions
        /// <inheritdoc/>
        public abstract IExpression[] InnerExpressions { get; }
        /// <inheritdoc/>
        public abstract string Build(QueryBuilderOptions options = QueryBuilderOptions.None);
        /// <inheritdoc/>
        public abstract void Build(StringBuilder builder, QueryBuilderOptions options = QueryBuilderOptions.None);

        /// <summary>
        /// The instance of the derived class inheriting from the current class.
        /// </summary>
        protected abstract TDerived Instance { get; }
        /// <summary>
        /// Adds a join expression to the current builder.
        /// </summary>
        /// <param name="joinExpression">The expression to add</param>
        protected abstract void AddJoinExpression(JoinExpression joinExpression);
        /// <summary>
        /// Adds a condition expression to the current builder.
        /// </summary>
        /// <param name="conditionExpression">The expression to add</param>
        protected abstract void AddConditionExpression(IConditionExpression conditionExpression);
    }
}
