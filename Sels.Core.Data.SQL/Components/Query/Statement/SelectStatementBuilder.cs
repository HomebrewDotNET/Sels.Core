﻿using Sels.Core.Data.SQL.Query.Compilation;
using Sels.Core.Data.SQL.Query.Expressions;
using Sels.Core.Data.SQL.Query.Expressions.Condition;
using Sels.Core.Data.SQL.Query.Expressions.Join;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Statement
{
    /// <summary>
    /// Builds a select query.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public class SelectStatementBuilder<TEntity> : BaseStatementBuilder<TEntity, SelectExpressionPositions, ISelectStatementBuilder<TEntity>>, ISelectStatementBuilder<TEntity>
    {
        // Properties
        /// <inheritdoc/>
        public override ISelectStatementBuilder<TEntity> Instance => this;

        /// <inheritdoc cref="SelectStatementBuilder{TEntity}"/>
        /// <param name="compiler">Compiler to create the query using the expressions defined in the current builder</param>
        public SelectStatementBuilder(IQueryCompiler<SelectExpressionPositions> compiler) : base(compiler)
        {
        }

        /// <inheritdoc cref="SelectStatementBuilder{TEntity}"/>
        /// <param name="compiler">Compiler to create the query using the expressions defined in the current builder</param>
        /// <param name="expressions">The expressions for the current query</param>
        public SelectStatementBuilder(IQueryCompiler<SelectExpressionPositions> compiler, Dictionary<SelectExpressionPositions, List<IExpression>> expressions) : base(compiler, expressions)
        {
        }

        #region Expressions        
        /// <inheritdoc/>
        public ISelectStatementBuilder<TEntity> Columns(object? dataset, IEnumerable<string> columns)
        {
            columns.ValidateArgumentNotNullOrEmpty(nameof(columns));

            columns.Execute(x => Expression(new ColumnExpression(dataset, x), SelectExpressionPositions.Column));
            return this;
        }
        /// <inheritdoc/>
        public ISelectStatementBuilder<TEntity> ColumnsOf<T>(object? dataset, params string[] excludedProperties)
        {
            foreach(var property in GetColumnPropertiesFrom<T>(excludedProperties))
            {
                Expression(new ColumnExpression(dataset, property.Name), SelectExpressionPositions.Column);
            }

            return this;
        }
        #endregion

        /// <inheritdoc/>
        protected override SelectExpressionPositions GetPositionForJoinExpression(JoinExpression<TEntity, ISelectStatementBuilder<TEntity>> joinExpression)
        {
            return SelectExpressionPositions.Join;
        }
        /// <inheritdoc/>
        protected override SelectExpressionPositions GetPositionForConditionExpression(ConditionGroupExpression<TEntity> conditionExpression)
        {
            return SelectExpressionPositions.Where;
        }
        /// <inheritdoc/>
        protected override ISelectStatementBuilder<TEntity> Clone(IQueryCompiler<SelectExpressionPositions> compiler, Dictionary<SelectExpressionPositions, List<IExpression>> expressions)
        {
            return new SelectStatementBuilder<TEntity>(compiler, expressions);
        }
    }
}
