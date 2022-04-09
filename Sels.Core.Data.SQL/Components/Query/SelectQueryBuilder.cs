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
    /// Builds select queries.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public class SelectQueryBuilder<TEntity> : BaseQueryBuilder<TEntity, ISelectQueryBuilder<TEntity>>, ISelectQueryBuilder<TEntity>
    {
        // Fields
        private readonly IQueryCompiler<SelectExpressionPositions> _compiler;
        private readonly Dictionary<SelectExpressionPositions, List<IExpression>> _expressions = new ();

        // Properties
        /// <inheritdoc/>
        public override IExpression[] InnerExpressions => _expressions.OrderBy(x => x.Key).SelectMany(x => x.Value).ToArray();
        /// <inheritdoc/>
        protected override ISelectQueryBuilder<TEntity> Instance => this;

        /// <inheritdoc cref="SelectQueryBuilder{TEntity}"/>
        /// <param name="compiler">Compiler to create the query using the expressions defined in the current builder</param>
        public SelectQueryBuilder(IQueryCompiler<SelectExpressionPositions> compiler)
        {
            _compiler = compiler.ValidateArgument(nameof(compiler));
        }

        #region Expressions
        /// <inheritdoc/>
        public ISelectQueryBuilder<TEntity> Expression(IExpression sqlExpression, SelectExpressionPositions location)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));

            _expressions.AddValueToList(location, sqlExpression);
            return this;
        }
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
            foreach(var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetIndexParameters()?.Length == 0 && (!excludedProperties.HasValue() || !excludedProperties.Contains(x.Name, StringComparer.OrdinalIgnoreCase))))
            {
                Expression(new ColumnExpression(dataset, property.Name), SelectExpressionPositions.Column);
            }

            return this;
        }
        #endregion

        #region Build
        /// <inheritdoc/>
        public override string Build(QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            var builder = new StringBuilder();
            Build(builder, options);
            return builder.ToString();
        }
        /// <inheritdoc/>
        public override void Build(StringBuilder builder, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));

            _compiler.CompileTo(builder, this, _expressions.ToDictionary(x => x.Key, x => x.Value.ToArray()), options);
        }
        #endregion

        /// <inheritdoc/>
        protected override void AddConditionExpression(ConditionGroupExpression<TEntity> conditionExpression)
        {
            conditionExpression.ValidateArgument(nameof(conditionExpression));

            Expression(conditionExpression, SelectExpressionPositions.Where);
        }
        /// <inheritdoc/>
        protected override void AddJoinExpression(JoinExpression<TEntity> joinExpression)
        {
            joinExpression.ValidateArgument(nameof(joinExpression));

            Expression(joinExpression, SelectExpressionPositions.Join);
        }
    }
}
