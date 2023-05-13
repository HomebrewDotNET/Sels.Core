using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that adds an alias using AS to another expression.
    /// </summary>
    public class AliasExpression : BaseExpressionContainer
    {
        // Properties
        /// <summary>
        /// The column alias.
        /// </summary>
        public string? Alias { get; set; }
        /// <summary>
        /// The expression that add the alias to.
        /// </summary>
        public IExpression Expression { get; }

        /// <inheritdoc cref="AliasExpression"/>
        /// <param name="expression"><inheritdoc cref="Expression"/></param>
        /// <param name="alias"><inheritdoc cref="Alias"/></param>
        public AliasExpression(IExpression expression, string? alias)
        {
            Expression = expression.ValidateArgument(nameof(expression));
            Alias = alias;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            subBuilder(builder, Expression);
            if (Alias.HasValue()) builder.Append(Sql.As).AppendSpace().Append(Alias);
        }
    }
}
