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
    /// Builds an insert query.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public class InsertQueryBuilder<TEntity> : BaseQueryBuilder<TEntity, InsertExpressionPositions, IInsertQueryBuilder<TEntity>>, IInsertQueryBuilder<TEntity>
    {
        /// <inheritdoc cref="InsertQueryBuilder{TEntity}"/>
        /// <param name="compiler">>Compiler to create the query using the expressions defined in the current builder</param>
        public InsertQueryBuilder(IQueryCompiler<InsertExpressionPositions> compiler) : base(compiler)
        {
        }

        #region Base Builder
        /// <inheritdoc/>
        public override IInsertQueryBuilder<TEntity> Instance => this;

        /// <inheritdoc/>
        protected override InsertExpressionPositions GetPositionForConditionExpression(ConditionGroupExpression<TEntity> conditionExpression)
        {
            throw new NotSupportedException("Where clause is not supported for an insert query");
        }
        /// <inheritdoc/>
        protected override InsertExpressionPositions GetPositionForJoinExpression(JoinExpression<TEntity> joinExpression)
        {
            throw new NotSupportedException("Join clause is not supported for an insert query");
        }
        #endregion

        #region Builder
        /// <inheritdoc/>
        public IInsertQueryBuilder<TEntity> Columns(IEnumerable<string> columns)
        {
            columns.ValidateArgumentNotNullOrEmpty(nameof(columns));

            var builder = this.Cast<IInsertQueryBuilder<TEntity>>();

            foreach (var column in columns)
            {
                builder.Column(column);
            }

            return this;
        }
        /// <inheritdoc/>
        public IInsertQueryBuilder<TEntity> ColumnsOf<T>(params string[] excludedProperties)
        {
            return Columns(GetColumnPropertiesFrom<T>(excludedProperties).Select(x => x.Name));
        }
        /// <inheritdoc/>
        public IInsertQueryBuilder<TEntity> Values(IEnumerable<object> values)
        {
            values.ValidateArgumentNotNullOrEmpty(nameof(values));

            return Expression(new ListExpression(values.Select(x => x is IExpression e ? e : new ConstantExpression(x))), InsertExpressionPositions.Values);
        }
        /// <inheritdoc/>
        public IInsertQueryBuilder<TEntity> ValuesUsing<T>(T valueObject, params string[] excludedProperties)
        {
            valueObject.ValidateArgument(nameof(valueObject));

            return Values(GetColumnPropertiesFrom<T>(excludedProperties).Select(x => x.GetValue(valueObject)));
        }
        /// <inheritdoc/>
        public IInsertQueryBuilder<TEntity> ParametersFrom<T>(int? suffix = null, params string[] excludedProperties)
        {
            var builder = this.Cast<IInsertQueryBuilder<TEntity>>();

            return builder.Parameters(GetColumnPropertiesFrom<T>(excludedProperties).Select(x => suffix.HasValue ? $"{x.Name}{suffix}" : x.Name));
        }
        #endregion
    }
}
