using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using System.Text;

namespace Sels.SQL.QueryBuilder.MySQL.Expressions.MariaDb
{
    /// <summary>
    /// Expression that represents the RETURNING keyword used to return values after inserting/deleting.
    /// Requires mariaDb 10.5.0 or later.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the expression for</typeparam>
    public class ReturningExpression<TEntity> : BaseExpressionContainer, IReturningExpressionBuilder<TEntity>
    {
        // Constants
        /// <summary>
        /// The MariaDb RETURNING keyword.
        /// </summary>
        public const string Keyword = "RETURNING";

        // Fields
        private readonly List<IExpression> _expressions = new List<IExpression>();

        // Properties
        /// <summary>
        /// Expressions containing the columns to return after insert or delete.
        /// </summary>
        public IExpression[] Expressions  => _expressions.ToArray();

        /// <inheritdoc cref="ReturningExpression{TEntity}"/>
        /// <param name="configurator">Delegate that builds this expression</param>
        /// <exception cref="InvalidOperationException"></exception>
        public ReturningExpression(Action<IReturningExpressionBuilder<TEntity>> configurator)
        {
            configurator.ValidateArgument(nameof(configurator));
            configurator(this);
            if (_expressions.Count == 0) throw new InvalidOperationException($"Expected {nameof(configurator)} to create at least 1 expression");
        }

        /// <inheritdoc/>
        public IReturningExpressionBuilder<TEntity> Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));

            _expressions.Add(expression);
            return this;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            builder.Append(Keyword).AppendSpace();
            Expressions.Execute((i, e) =>
            {
                subBuilder(builder, e);
                if (i < Expressions.Length - 1) builder.Append(',');
            });
        }
    }
    /// <summary>
    /// Builder for configuring  a <see cref="ReturningExpression{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the expression for</typeparam>
    public interface IReturningExpressionBuilder<TEntity> : ISharedExpressionBuilder<TEntity, IReturningExpressionBuilder<TEntity>>
    {
        /// <summary>
        /// Return all columns.
        /// </summary>
        void All() => Expression(new AllColumnsExpression());
    }
}
