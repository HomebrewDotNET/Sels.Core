using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that wraps another expression using prefixes and/or suffixes.
    /// </summary>
    public class WrapperExpression : BaseExpressionContainer
    {
        // Properties
        /// <summary>
        /// Optional values to add in front of <see cref="Expression"/>.
        /// </summary>
        public IEnumerable<object> Prefixes { get; }
        /// <summary>
        /// The wrapped expression.
        /// </summary>
        public IExpression Expression { get; }
        /// <summary>
        /// Optional values to add after <see cref="Expression"/>
        /// </summary>
        public IEnumerable<object> Suffixes { get; }

        /// <inheritdoc cref="WrapperExpression"/>
        /// <param name="prefixes"><inheritdoc cref="Prefixes"/></param>
        /// <param name="expression"><inheritdoc cref="Expression"/></param>
        /// <param name="suffixes"><inheritdoc cref="Suffixes"/></param>
        public WrapperExpression(IEnumerable<object> prefixes, IExpression expression, IEnumerable<object> suffixes)
        {
            Prefixes = prefixes;
            Expression = expression.ValidateArgument(nameof(expression));
            Suffixes = suffixes;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            Prefixes.Execute(x => builder.Append(x));
            subBuilder(builder, Expression);
            Suffixes.Execute(x => builder.Append(x));
        }
    }
}
