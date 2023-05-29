using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <inheritdoc cref="ICteStatementBuilder"/>
    public class CteStatementBuilder : BaseExpressionBuilder, ICteStatementBuilder, ICteOrSelectStatementBuilder, IExpressionContainer
    {
        // Fields
        private readonly List<IExpression> _expressions = new List<IExpression>();
        private IExpression _queryExpression;

        // Properties
        /// <inheritdoc/>
        public override IExpression[] InnerExpressions => Helper.Collection.EnumerateAll(_expressions, _queryExpression.AsArrayOrDefault()).ToArray();
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
        public IQueryBuilder Execute(Action<StringBuilder, ExpressionCompileOptions> query)
        {
            query.ValidateArgument(nameof(query));

            _queryExpression = new SubQueryExpression(null, query, false, true);
            return this;
        }

        #region Expression
        /// <inheritdoc/>
        public void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            if (!_expressions.HasValue()) throw new InvalidOperationException($"No cte expressions defined");
            if (_queryExpression == null) throw new InvalidOperationException($"No select query defined");
            var isFormatted = options.HasFlag(ExpressionCompileOptions.Format);

            builder.Append(Sql.With).AppendSpace();

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

            // Build query
            if (isFormatted) builder.AppendLine(); else builder.AppendSpace();
            subBuilder(builder, _queryExpression);
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
