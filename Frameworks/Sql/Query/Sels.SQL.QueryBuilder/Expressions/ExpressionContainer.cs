using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions.Linq;
using Sels.Core;
using Sels.Core.Extensions;

namespace Sels.SQL.QueryBuilder.Expressions
{
    /// <summary>
    /// Expression that consists of other expressions.
    /// </summary>
    public class ExpressionContainer : BaseExpressionContainer
    {
        // Properties
        /// <summary>
        /// The inner expression for the current expression.
        /// </summary>
        public List<IExpression> InnerExpressions { get;} = new List<IExpression>();
        /// <summary>
        /// The value used to join together <see cref="InnerExpressions"/>.
        /// </summary>
        public object JoinValue { get; set; } = Constants.Strings.Space;

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));
            if (!InnerExpressions.HasValue()) return;

            InnerExpressions.Execute((i, e) => {
                subBuilder(builder, e);

                if (i != InnerExpressions.Count - 1 && JoinValue != null) builder.Append(JoinValue);
            });
        }
    }
}
