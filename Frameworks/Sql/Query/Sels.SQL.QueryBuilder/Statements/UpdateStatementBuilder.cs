using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builds an update query.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public class UpdateStatementBuilder<TEntity> : BaseStatementBuilder<TEntity, UpdateExpressionPositions, IUpdateStatementBuilder<TEntity>>, 
        IUpdateStatementBuilder<TEntity>,
        ISharedExpressionBuilder<TEntity, IStatementSetToBuilder<TEntity, ISharedExpressionBuilder<TEntity, IUpdateStatementBuilder<TEntity>>>>
    {
        /// <inheritdoc cref="UpdateStatementBuilder{TEntity}"/>
        /// <param name="compiler">Compiler to create the query using the expressions defined in the current builder</param>
        public UpdateStatementBuilder(IQueryCompiler<UpdateExpressionPositions> compiler) : base(compiler)
        {
        }

        /// <inheritdoc cref="UpdateStatementBuilder{TEntity}"/>
        /// <param name="compiler">Compiler to create the query using the expressions defined in the current builder</param>
        /// <param name="expressions">The expressions for the current query</param>
        public UpdateStatementBuilder(IQueryCompiler<UpdateExpressionPositions> compiler, Dictionary<UpdateExpressionPositions, List<OrderedExpression>> expressions) : base(compiler, expressions)
        {
        }

        #region Base builder
        /// <inheritdoc/>
        public override IUpdateStatementBuilder<TEntity> Instance => this;

        /// <inheritdoc/>
        protected override UpdateExpressionPositions GetPositionForConditionExpression(ConditionGroupExpression<TEntity> conditionExpression)
        {
            return UpdateExpressionPositions.Where;
        }
        /// <inheritdoc/>
        protected override UpdateExpressionPositions GetPositionForJoinExpression(JoinExpression<TEntity, IUpdateStatementBuilder<TEntity>> joinExpression)
        {
            return UpdateExpressionPositions.Join;
        }
        /// <inheritdoc/>
        protected override IUpdateStatementBuilder<TEntity> Clone(IQueryCompiler<UpdateExpressionPositions> compiler, Dictionary<UpdateExpressionPositions, List<OrderedExpression>> expressions)
        {
            return new UpdateStatementBuilder<TEntity>(compiler, expressions);
        }
        #endregion

        #region Builder
        /// <inheritdoc/>
        public ISharedExpressionBuilder<TEntity, IStatementSetToBuilder<TEntity, ISharedExpressionBuilder<TEntity, IUpdateStatementBuilder<TEntity>>>> Set => this;
        /// <inheritdoc/>
        public IStatementSetToBuilder<TEntity, ISharedExpressionBuilder<TEntity, IUpdateStatementBuilder<TEntity>>> Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));

            var fullExpression = new SetExpression<TEntity, IUpdateStatementBuilder<TEntity>>(this, expression);
            Expression(fullExpression, UpdateExpressionPositions.Set);
            return fullExpression;
        }
        /// <inheritdoc/>
        public IUpdateStatementBuilder<TEntity> SetFrom<T>(object dataset = null, params string[] excludedProperties)
        {
            var builder = this.CastTo<IUpdateStatementBuilder<TEntity>>();
            foreach(var property in GetColumnPropertiesFrom<T>(excludedProperties))
            {
                builder.Set.Column(dataset ?? typeof(T), property.Name).To.Parameter(property.Name);
            }

            return this;
        }
        /// <inheritdoc/>
        public IUpdateStatementBuilder<TEntity> SetUsing<T>(T valueObject, object dataset = null, params string[] excludedProperties)
        {
            valueObject.ValidateArgument(nameof(valueObject));

            var builder = this.CastTo<IUpdateStatementBuilder<TEntity>>();
            foreach (var property in GetColumnPropertiesFrom<T>(excludedProperties))
            {
                builder.Set.Column(dataset ?? typeof(T), property.Name).To.Value(property.GetValue(valueObject));
            }

            return this;
        }
        #endregion

        /// <inheritdoc/>
        public override StringBuilder Build(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            // Auto assume no implicit 
            if (typeof(TEntity) == typeof(object)) options |= ExpressionCompileOptions.NoImplitExpressions;

            // Add implicit expressions
            if (!options.HasFlag(ExpressionCompileOptions.NoImplitExpressions) && (!Expressions.ContainsKey(UpdateExpressionPositions.Table) || !Expressions[UpdateExpressionPositions.Table].HasValue())) this.CastTo<IUpdateStatementBuilder<TEntity>>().Table();

            return base.Build(builder, options);
        }
    }
}
