using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.SQL.QueryBuilder.Expressions
{
    /// <summary>
    /// Sql expression that wraps another expression in ().
    /// </summary>
    public class WrappedExpression : WrapperExpression
    {
        // Statics
        private static object[] _prefix = new object[] { '(' };
        private static object[] _suffix = new object[] { ')' };

        /// <inheritdoc cref="WrappedExpression"/>
        /// <param name="innerExpression">The expression to wrap</param>
        public WrappedExpression(IExpression innerExpression) : base(_prefix, innerExpression.ValidateArgument(nameof(innerExpression)), _suffix)
        {
        }
    }
}
