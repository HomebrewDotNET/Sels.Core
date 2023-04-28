﻿using Sels.Core.Data.SQL.Query.Compilation;
using Sels.Core.Data.SQL.Query.Expressions;
using Sels.Core.Data.SQL.Query.Expressions.Condition;
using Sels.Core.Data.SQL.Query.Expressions.Join;
using System.Text;

namespace Sels.Core.Data.SQL.Query.Statement
{
    /// <summary>
    /// Builds an insert query.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public class InsertStatementBuilder<TEntity> : BaseStatementBuilder<TEntity, InsertExpressionPositions, IInsertStatementBuilder<TEntity>>, IInsertStatementBuilder<TEntity>
    {
        /// <inheritdoc cref="InsertStatementBuilder{TEntity}"/>
        /// <param name="compiler">>Compiler to create the query using the expressions defined in the current builder</param>
        public InsertStatementBuilder(IQueryCompiler<InsertExpressionPositions> compiler) : base(compiler)
        {
        }

        /// <inheritdoc cref="InsertStatementBuilder{TEntity}"/>
        /// <param name="compiler">>Compiler to create the query using the expressions defined in the current builder</param>
        /// <param name="expressions">The expressions for the current query</param>
        public InsertStatementBuilder(IQueryCompiler<InsertExpressionPositions> compiler, Dictionary<InsertExpressionPositions, List<IExpression>> expressions) : base(compiler, expressions)
        {
        }

        #region Base Builder
        /// <inheritdoc/>
        public override IInsertStatementBuilder<TEntity> Instance => this;
        /// <inheritdoc/>
        protected override IInsertStatementBuilder<TEntity> Clone(IQueryCompiler<InsertExpressionPositions> compiler, Dictionary<InsertExpressionPositions, List<IExpression>> expressions)
        {
            return new InsertStatementBuilder<TEntity>(compiler, expressions);
        }

        /// <inheritdoc/>
        protected override InsertExpressionPositions GetPositionForConditionExpression(ConditionGroupExpression<TEntity> conditionExpression)
        {
            throw new NotSupportedException("Where clause is not supported for an insert query");
        }
        /// <inheritdoc/>
        protected override InsertExpressionPositions GetPositionForJoinExpression(JoinExpression<TEntity, IInsertStatementBuilder<TEntity>> joinExpression)
        {
            throw new NotSupportedException("Join clause is not supported for an insert query");
        }
        #endregion

        #region Builder
        /// <inheritdoc/>
        public IInsertStatementBuilder<TEntity> Columns(IEnumerable<string> columns)
        {
            columns.ValidateArgumentNotNullOrEmpty(nameof(columns));

            var builder = this.CastTo<IInsertStatementBuilder<TEntity>>();

            foreach (var column in columns)
            {
                builder.Column(column);
            }

            return this;
        }
        /// <inheritdoc/>
        public IInsertStatementBuilder<TEntity> ColumnsOf<T>(params string[] excludedProperties)
        {
            return Columns(GetColumnPropertiesFrom<T>(excludedProperties).Select(x => x.Name));
        }
        /// <inheritdoc/>
        public IInsertStatementBuilder<TEntity> Values(IEnumerable<object> values)
        {
            values.ValidateArgumentNotNullOrEmpty(nameof(values));

            return Expression(new ListExpression(values.Select(x => x is IExpression e ? e : new ConstantExpression(x))), InsertExpressionPositions.Values);
        }
        /// <inheritdoc/>
        public IInsertStatementBuilder<TEntity> ValuesUsing<T>(T valueObject, params string[] excludedProperties)
        {
            valueObject.ValidateArgument(nameof(valueObject));

            return Values(GetColumnPropertiesFrom<T>(excludedProperties).Select(x => x.GetValue(valueObject)));
        }
        /// <inheritdoc/>
        public IInsertStatementBuilder<TEntity> ParametersFrom<T>(int? suffix = null, params string[] excludedProperties)
        {
            var builder = this.CastTo<IInsertStatementBuilder<TEntity>>();

            return builder.Parameters(GetColumnPropertiesFrom<T>(excludedProperties).Select(x => suffix.HasValue ? $"{x.Name}{suffix}" : x.Name));
        }
        #endregion

        /// <inheritdoc/>
        public override StringBuilder Build(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            // Add implicit expressions
            if (!options.HasFlag(ExpressionCompileOptions.NoImplitExpressions) && (!Expressions.ContainsKey(InsertExpressionPositions.Into) || !Expressions[InsertExpressionPositions.Into].HasValue())) this.CastTo<IInsertStatementBuilder<TEntity>>().Into();

            return base.Build(builder, options);
        }
    }
}
