using System;
using System.Text;
using Sels.Core.Attributes.Enumeration.Value;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that represents a column to sort a query by.
    /// </summary>
    public class OrderByExpression : BaseExpressionContainer
    {
        /// <summary>
        /// The order to sort in.
        /// </summary>
        public SortOrders? SortOrder { get; }
        /// <summary>
        /// Expression that contains the expression to sort by.
        /// </summary>
        public IExpression Expression { get; set; }

        /// <inheritdoc cref="ColumnFunctionExpression"/>
        /// <param name="sortOrder"><inheritdoc cref="SortOrder"/></param>
        /// <param name="expression"><inheritdoc cref="Expression"/></param>
        public OrderByExpression(IExpression expression, SortOrders? sortOrder = null)
        {
            Expression = expression.ValidateArgument(nameof(expression));
            SortOrder = sortOrder;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            subBuilder(builder, Expression);
            if (SortOrder != null) builder.AppendSpace().Append(SortOrder.GetStringValue());
        }
    }
}
