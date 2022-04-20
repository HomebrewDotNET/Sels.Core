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

namespace Sels.Core.Data.SQL.Query
{
    /// <summary>
    /// Builds an update query.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public class UpdateQueryBuilder<TEntity> : BaseQueryBuilder<TEntity, UpdateExpressionPositions, IUpdateQueryBuilder<TEntity>>, IUpdateQueryBuilder<TEntity>
    {
        /// <inheritdoc cref="UpdateQueryBuilder{TEntity}"/>
        /// <param name="compiler">Compiler to create the query using the expressions defined in the current builder</param>
        public UpdateQueryBuilder(IQueryCompiler<UpdateExpressionPositions> compiler) : base(compiler)
        {
        }

        #region Base builder
        /// <inheritdoc/>
        protected override IUpdateQueryBuilder<TEntity> Instance => this;

        /// <inheritdoc/>
        protected override UpdateExpressionPositions GetPositionForConditionExpression(ConditionGroupExpression<TEntity> conditionExpression)
        {
            return UpdateExpressionPositions.Where;
        }
        /// <inheritdoc/>
        protected override UpdateExpressionPositions GetPositionForJoinExpression(JoinExpression<TEntity> joinExpression)
        {
            return UpdateExpressionPositions.Join;
        }
        #endregion

        #region Builder
        /// <inheritdoc/>
        public ISharedExpressionBuilder<TEntity, IUpdateQueryBuilder<TEntity>> SetTo(IExpression sqlExpression)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));

            var fullExpression = new SetExpression<TEntity, IUpdateQueryBuilder<TEntity>>(this, sqlExpression);
            Expression(fullExpression, UpdateExpressionPositions.Set);
            return fullExpression;
        }
        /// <inheritdoc/>
        public IUpdateQueryBuilder<TEntity> SetFrom<T>(object? dataset = null, params string[] excludedProperties)
        {
            var builder = this.Cast<IUpdateQueryBuilder<TEntity>>();
            foreach(var property in GetColumnPropertiesFrom<T>(excludedProperties))
            {
                builder.SetColumnTo(dataset ?? typeof(T), property.Name).Parameter(property.Name);
            }

            return this;
        }
        /// <inheritdoc/>
        public IUpdateQueryBuilder<TEntity> SetUsing<T>(T valueObject, object? dataset = null, params string[] excludedProperties)
        {
            valueObject.ValidateArgument(nameof(valueObject));

            var builder = this.Cast<IUpdateQueryBuilder<TEntity>>();
            foreach (var property in GetColumnPropertiesFrom<T>(excludedProperties))
            {
                builder.SetColumnTo(dataset ?? typeof(T), property.Name).Value(property.GetValue(valueObject));
            }

            return this;
        }
        #endregion
    }
}
