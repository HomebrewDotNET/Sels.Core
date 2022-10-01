using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that represents the DISTINCT sql keyword.
    /// </summary>
    public class DistintExpression : BaseExpression
    {
        /// <summary>
        /// The distint keyword.
        /// </summary>
        public const string Keyword = "DISTINCT";

        private DistintExpression()
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
        public static DistintExpression Instance { get; } = new DistintExpression();
    }
}
