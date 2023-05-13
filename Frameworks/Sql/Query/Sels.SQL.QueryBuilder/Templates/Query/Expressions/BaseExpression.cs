using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Template for creating a <see cref="IExpression"/>.
    /// </summary>
    public abstract class BaseExpression : IExpression
    {
        /// <inheritdoc/>
        public abstract void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None);
        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();
            ToSql(builder, ExpressionCompileOptions.Format);
            return builder.ToString();
        }
    }
}
