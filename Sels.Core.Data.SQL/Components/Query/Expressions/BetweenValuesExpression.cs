using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression containing the sql values for the between operator.
    /// </summary>
    public class BetweenValuesExpression : BaseExpressionContainer
    {
        // Properties
        /// <summary>
        /// Expression containing the lower value.
        /// </summary>
        public IExpression LowerExpression { get; }
        /// <summary>
        /// Expression containing the top value.
        /// </summary>
        public IExpression TopExpression { get; }

        /// <inheritdoc cref="BetweenValuesExpression"/>
        /// <param name="lowerExpression"><inheritdoc cref="LowerExpression"/></param>
        /// <param name="topExpression"><inheritdoc cref="TopExpression"/></param>
        public BetweenValuesExpression(IExpression lowerExpression, IExpression topExpression)
        {
            LowerExpression = lowerExpression.ValidateArgument(nameof(lowerExpression));
            TopExpression = topExpression.ValidateArgument(nameof(topExpression));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, QueryBuilderOptions options = QueryBuilderOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            subBuilder(builder, LowerExpression);

            builder.AppendSpace().Append(Sql.LogicOperators.And).AppendSpace();

            subBuilder(builder, TopExpression);
        }
    }
}
