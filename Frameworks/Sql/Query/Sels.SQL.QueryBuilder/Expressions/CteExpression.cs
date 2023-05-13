using Sels.SQL.QueryBuilder.Builder.Statement;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that represents a common table expression.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to map to the cte columns</typeparam>
    public class CteExpression<TEntity> : BaseExpressionContainer, ICteExpressionBuilder<TEntity>
    {
        // Fields
        private readonly IObjectExpression _name;
        private readonly List<IExpression> _columnExpressions = new List<IExpression>();
        private IExpression _queryExpression;

        // Properties
        /// <summary>
        /// The builder to return when done building the current expression.
        /// </summary>
        public ICteOrSelectStatementBuilder Builder { get; set; }

        /// <inheritdoc cref="CteExpression{TEntity}"/>
        /// <param name="builder">The builder to return after this expression has been built</param>
        /// <param name="name">The name of this cte</param>
        public CteExpression(ICteOrSelectStatementBuilder builder, string name)
        {
             name.ValidateArgumentNotNullOrWhitespace(nameof(name));
            _name = new ObjectExpression(null, name);
            Builder = builder.ValidateArgument(nameof(builder));
        }

        /// <inheritdoc cref="CteExpression{TEntity}"/>
        /// <param name="name">The name of this cte</param>
        public CteExpression(string name)
        {
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));
            _name = new ObjectExpression(null, name);
        }

        /// <inheritdoc/>
        public ICteExpressionBuilder<TEntity> Column(string column)
        {
            column.ValidateArgumentNotNullOrWhitespace(nameof(column));

            _columnExpressions.Add(new ColumnExpression(null, column));
            return this;
        }
        /// <inheritdoc/>
        public ICteOrSelectStatementBuilder Using(Func<ExpressionCompileOptions, string> query)
        {
            query.ValidateArgument(nameof(query));
            if (Builder == null) throw new InvalidOperationException("Builder is not set");

            _queryExpression = new SubQueryExpression(null, query, false);
            return Builder;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            if(_queryExpression == null) throw new InvalidOperationException("No query expression is defined");

            var isFormatted = options.HasFlag(ExpressionCompileOptions.Format);

            subBuilder(builder, _name);
            builder.AppendSpace();

            // Add columns if present
            if (_columnExpressions.HasValue())
            {
                builder.Append('(');

                _columnExpressions.Execute((i, e) =>
                {
                    subBuilder(builder, e);
                    if (i < _columnExpressions.Count - 1)
                    {
                        builder.Append(',');
                    }
                });

                builder.Append(')').AppendSpace();
            }

            builder.Append(Sql.As).AppendSpace().Append('(');
            // Add query
            if (isFormatted) builder.AppendLine();
            subBuilder(builder, _queryExpression);
            if (isFormatted) builder.AppendLine();
            builder.Append(')');
        }
    }
}
