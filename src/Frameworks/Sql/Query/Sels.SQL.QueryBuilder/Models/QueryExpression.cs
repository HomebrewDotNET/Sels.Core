using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.SQL.QueryBuilder
{
    /// <summary>
    /// Expression that has an order in a query based on the position it's in.
    /// </summary>
    public class OrderedExpression
    {
        // Properties
        /// <summary>
        /// Sql expression.
        /// </summary>
        public IExpression Expression { get; }
        /// <summary>
        /// Optional order for <see cref="Expression"/>. A lower order means it will be compiled first. Can be used to sort custom expressions.
        /// </summary>
        public int Order { get; }

        /// <inheritdoc cref="OrderedExpression"/>
        /// <param name="expression"><inheritdoc cref="Expression"/></param>
        /// <param name="order"><inheritdoc cref="Order"/></param>
        public OrderedExpression(IExpression expression, int order = 0)
        {
            Expression = expression.ValidateArgument(nameof(expression));
            Order = order;
        }
    }
}
