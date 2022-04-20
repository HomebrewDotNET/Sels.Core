using Sels.Core.Data.SQL.Query.Compilation;
using Sels.Core.Data.SQL.Query.Expressions;
using Sels.Core.Data.SQL.Query.Expressions.Condition;
using Sels.Core.Data.SQL.Query.Expressions.Join;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query
{
    /// <summary>
    /// Builds a select query.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public class SelectQueryBuilder<TEntity> : BaseQueryBuilder<TEntity, SelectExpressionPositions, ISelectQueryBuilder<TEntity>>, ISelectQueryBuilder<TEntity>
    {
        // Properties
        /// <inheritdoc/>
        protected override ISelectQueryBuilder<TEntity> Instance => this;

        /// <inheritdoc cref="SelectQueryBuilder{TEntity}"/>
        /// <param name="compiler">Compiler to create the query using the expressions defined in the current builder</param>
        public SelectQueryBuilder(IQueryCompiler<SelectExpressionPositions> compiler) : base(compiler)
        {
        }

        #region Expressions        
        /// <inheritdoc/>
        public ISelectQueryBuilder<TEntity> Columns(object? dataset, IEnumerable<string> columns)
        {
            columns.ValidateArgumentNotNullOrEmpty(nameof(columns));

            columns.Execute(x => Expression(new ColumnExpression(dataset, x), SelectExpressionPositions.Column));
            return this;
        }
        /// <inheritdoc/>
        public ISelectQueryBuilder<TEntity> ColumnsOf<T>(object? dataset, params string[] excludedProperties)
        {
            foreach(var property in GetColumnPropertiesFrom<T>(excludedProperties))
            {
                Expression(new ColumnExpression(dataset, property.Name), SelectExpressionPositions.Column);
            }

            return this;
        }
        #endregion

        /// <inheritdoc/>
        protected override SelectExpressionPositions GetPositionForJoinExpression(JoinExpression<TEntity> joinExpression)
        {
            return SelectExpressionPositions.Join;
        }
        /// <inheritdoc/>
        protected override SelectExpressionPositions GetPositionForConditionExpression(ConditionGroupExpression<TEntity> conditionExpression)
        {
            return SelectExpressionPositions.Where;
        }
    }
}
