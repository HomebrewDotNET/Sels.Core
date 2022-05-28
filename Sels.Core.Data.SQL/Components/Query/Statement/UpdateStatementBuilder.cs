using Sels.Core.Data.SQL.Query.Compilation;
using Sels.Core.Data.SQL.Query.Expressions;
using Sels.Core.Data.SQL.Query.Expressions.Condition;
using Sels.Core.Data.SQL.Query.Expressions.Join;
using Sels.Core.Data.SQL.Query.Expressions.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Statement
{
    /// <summary>
    /// Builds an update query.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public class UpdateStatementBuilder<TEntity> : BaseStatementBuilder<TEntity, UpdateExpressionPositions, IUpdateStatementBuilder<TEntity>>, IUpdateStatementBuilder<TEntity>
    {
        /// <inheritdoc cref="UpdateStatementBuilder{TEntity}"/>
        /// <param name="compiler">Compiler to create the query using the expressions defined in the current builder</param>
        public UpdateStatementBuilder(IQueryCompiler<UpdateExpressionPositions> compiler) : base(compiler)
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
        #endregion

        #region Builder
        /// <inheritdoc/>
        public IStatementSetToBuilder<TEntity, ISharedExpressionBuilder<TEntity, IUpdateStatementBuilder<TEntity>>> SetExpression(IExpression sqlExpression)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));

            var fullExpression = new SetExpression<TEntity, IUpdateStatementBuilder<TEntity>>(this, sqlExpression);
            Expression(fullExpression, UpdateExpressionPositions.Set);
            return fullExpression;
        }
        /// <inheritdoc/>
        public IUpdateStatementBuilder<TEntity> SetFrom<T>(object? dataset = null, params string[] excludedProperties)
        {
            var builder = this.Cast<IUpdateStatementBuilder<TEntity>>();
            foreach(var property in GetColumnPropertiesFrom<T>(excludedProperties))
            {
                builder.Set(dataset ?? typeof(T), property.Name).To.Parameter(property.Name);
            }

            return this;
        }
        /// <inheritdoc/>
        public IUpdateStatementBuilder<TEntity> SetUsing<T>(T valueObject, object? dataset = null, params string[] excludedProperties)
        {
            valueObject.ValidateArgument(nameof(valueObject));

            var builder = this.Cast<IUpdateStatementBuilder<TEntity>>();
            foreach (var property in GetColumnPropertiesFrom<T>(excludedProperties))
            {
                builder.Set(dataset ?? typeof(T), property.Name).To.Value(property.GetValue(valueObject));
            }

            return this;
        }
        #endregion
    }
}
