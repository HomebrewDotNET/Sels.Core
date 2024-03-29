﻿using Sels.Core.Data.SQL.Query.Compilation;
using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Statement
{
    /// <inheritdoc cref="ICteStatementBuilder"/>
    public class CteStatementBuilder : BaseExpressionBuilder, ICteStatementBuilder, ICteOrSelectStatementBuilder, IExpressionContainer
    {
        // Fields
        private readonly List<IExpression> _expressions = new List<IExpression>();
        private IExpression _selectQueryExpression;

        // Properties
        /// <inheritdoc/>
        public override IExpression[] InnerExpressions => Helper.Collection.EnumerateAll(_expressions, _selectQueryExpression.AsArrayOrDefault()).ToArray();
        /// <inheritdoc/>
        protected override IExpression Expression => this;

        /// <inheritdoc cref="CteStatementBuilder"/>
        /// <param name="compiler">Compiler for compiling this builder into sql</param>
        public CteStatementBuilder(IExpressionCompiler compiler) : base(compiler)
        {
        }

        /// <inheritdoc/>
        ICteOrSelectStatementBuilder ICteStatementBuilder.Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));

            _expressions.Add(expression);
            return this;
        }
        /// <inheritdoc/>
        public ICteExpressionBuilder<T> Cte<T>(string name)
        {
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            var cteExpression = new CteExpression<T>(this, name);
            _expressions.Add(cteExpression);

            return cteExpression;
        }
        /// <inheritdoc/>
        public IQueryBuilder Execute(Func<ExpressionCompileOptions, string> query)
        {
            query.ValidateArgument(nameof(query));

            _selectQueryExpression = new SubQueryExpression(null, query, false);
            return this;
        }

        #region Expression
        /// <inheritdoc/>
        public void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            if (!_expressions.HasValue()) throw new InvalidOperationException($"No cte expressions defined");
            if (_selectQueryExpression == null) throw new InvalidOperationException($"No select query defined");
            var isFormatted = options.HasFlag(ExpressionCompileOptions.Format);

            builder.Append(Sql.With).AppendSpace();
            if (isFormatted) builder.AppendLine(); else builder.AppendSpace();

            // Build cte's
            _expressions.Execute((i, e) =>
            {
                subBuilder(builder, e);
                if (i < _expressions.Count - 1)
                {
                    builder.Append(',');
                    if (isFormatted) builder.AppendLine(); else builder.AppendSpace();
                }
            });

            // Build select query
            if (isFormatted) builder.AppendLine(); else builder.AppendSpace();
            subBuilder(builder, _selectQueryExpression);
        }
        /// <inheritdoc/>
        public void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));

            ToSql(builder, (b, e) => e.ToSql(b, options), options);
        }
        #endregion
    }
}
