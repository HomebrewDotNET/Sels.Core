using Sels.Core.Data.SQL.Query.Statement;
using System.Text;

namespace Sels.Core.Data.SQL.Query.Expressions.Update
{
    /// <summary>
    /// Expression that represents a set expression in an update query where a sql object is updated to a new value.
    /// </summary>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    /// <typeparam name="TEntity">The main entity to update</typeparam>
    public class SetExpression<TEntity, TDerived> : BaseExpressionContainer, IStatementSetToBuilder<TEntity, ISharedExpressionBuilder<TEntity, TDerived>>, ISharedExpressionBuilder<TEntity, TDerived>
    {
        // Fields
        private readonly TDerived _builder;

        // Properties
        /// <summary>
        /// Expression containing the object to update.
        /// </summary>
        public IExpression LeftExpression { get; }
        /// <summary>
        /// Expression containing the value to update <see cref="LeftExpression"/> to.
        /// </summary>
        public IExpression RightExpression { get; private set; }
        /// <inheritdoc/>
        public ISharedExpressionBuilder<TEntity, TDerived> To => this;

        /// <inheritdoc cref="SetExpression{TEntity, TDerived}"/>
        /// <param name="builder">The builder to return after selecting <see cref="RightExpression"/></param>
        /// <param name="leftExpression"><inheritdoc cref="LeftExpression"/></param>
        public SetExpression(TDerived builder, IExpression leftExpression)
        {
            _builder = builder.ValidateArgument(nameof(builder));
            LeftExpression = leftExpression.ValidateArgument(nameof(leftExpression));
        }

        /// <inheritdoc />
        public TDerived Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            if (RightExpression != null) throw new InvalidOperationException("Expected right expression to be null but was not");

            RightExpression = expression;
            return _builder;
        }

        /// <inheritdoc />
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));
            if (RightExpression == null) throw new InvalidOperationException($"{nameof(RightExpression)} is not set");

            subBuilder(builder, LeftExpression);
            builder.AppendSpace().Append('=').AppendSpace();
            subBuilder(builder, RightExpression);
        }
    }
}
