using Sels.Core.Extensions;
using System;
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
        /// The expression containing the alias.
        /// </summary>
        public IExpression Alias { get; set; }
        /// <summary>
        /// The expression to add the alias to.
        /// </summary>
        public IExpression Expression { get; }

        /// <inheritdoc cref="AliasExpression"/>
        /// <param name="expression"><inheritdoc cref="Expression"/></param>
        /// <param name="alias">The alias</param>
        public AliasExpression(IExpression expression, string alias) : this(expression, alias.HasValue() ? new ObjectExpression(null, alias) : null)
        {
        }

        /// <inheritdoc cref="AliasExpression"/>
        /// <param name="expression"><inheritdoc cref="Expression"/></param>
        /// <param name="aliasExpression"><inheritdoc cref="Alias"/></param>
        public AliasExpression(IExpression expression, IExpression aliasExpression)
        {
            Expression = expression.ValidateArgument(nameof(expression));
            Alias = aliasExpression;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            subBuilder(builder, Expression);
            if (Alias.HasValue()) {
                builder.AppendSpace().Append(Sql.As).AppendSpace();
                subBuilder(builder, Alias);
            } 
        }
    }
}
