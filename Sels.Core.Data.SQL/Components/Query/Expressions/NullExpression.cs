using Sels.Core.Data.SQL.Query;
using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
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
        public override void ToSql(StringBuilder builder, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            
        }

        // Statics
        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static NullExpression Value { get; } = new NullExpression();
    }
}
