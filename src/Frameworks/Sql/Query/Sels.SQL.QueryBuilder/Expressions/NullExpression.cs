using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that doesn't add any sql.
    /// </summary>
    public class NullExpression : BaseExpression, IExpression
    {
        private NullExpression()
        {

        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            
        }

        // Statics
        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static NullExpression Value { get; } = new NullExpression();
    }
}
