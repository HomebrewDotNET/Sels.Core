using System.Text;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that converts the supplied object to a sql string using <see cref="object.ToString()"/>.
    /// </summary>
    public class RawExpression : BaseExpression, IExpression
    {
        /// <summary>
        /// The object containing the sql expression.
        /// </summary>
        public object Expression { get; }

        /// <inheritdoc cref="RawExpression"/>
        /// <param name="expression"><inheritdoc cref="Expression"/></param>
        public RawExpression(object expression)
        {
            Expression = expression.ValidateArgument(nameof(expression));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            builder.Append(Expression);
        }
    }
}
