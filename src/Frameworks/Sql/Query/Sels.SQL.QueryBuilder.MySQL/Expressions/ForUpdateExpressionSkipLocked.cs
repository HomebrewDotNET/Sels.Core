using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System.Text;

namespace Sels.SQL.QueryBuilder.MySQL.Expressions
{
    /// <summary>
    /// Expressions that represents the FOR UPDATE SKIP LOCKED keyword for locking a row during a select but skipping locked rows.
    /// </summary>
    public class ForUpdateExpressionSkipLocked : BaseExpression
    {
        /// <summary>
        /// The MySql for update keyword.
        /// </summary>
        public const string Keyword = ForUpdateExpression.Keyword + " SKIP LOCKED";

        private ForUpdateExpressionSkipLocked()
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
        public static ForUpdateExpressionSkipLocked Instance { get; } = new ForUpdateExpressionSkipLocked();
    }
}
