using Sels.Core.Data.SQL.Query;
using Sels.Core.Data.SQL.Query.Expressions;
using System.Text;

namespace Sels.Core.Data.MySQL.Query.Expressions
{
    /// <summary>
    /// Expressions that represents the FOR UPDATE keyword for locking a row during a select.
    /// </summary>
    public class ForUpdateExpression : BaseExpression
    {
        /// <summary>
        /// The MySql for update keyword.
        /// </summary>
        public const string Keyword = "FOR UPDATE";

        private ForUpdateExpression()
        {

        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));

            builder.Append(Keyword);
        }

        // Statics
        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static ForUpdateExpression Instance { get; } = new ForUpdateExpression();
    }
}
