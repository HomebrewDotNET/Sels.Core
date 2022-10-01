using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that wraps another expression using strings.
    /// </summary>
    public class WrapperExpression : BaseExpressionContainer
    {
        // Properties
        /// <summary>
        /// Optional strings to add in front of <see cref="Expression"/>.
        /// </summary>
        public IEnumerable<string>? Prefixes { get; }
        /// <summary>
        /// The wrapped expression.
        /// </summary>
        public IExpression Expression { get; }
        /// <summary>
        /// Optional strings to add after <see cref="Expression"/>
        /// </summary>
        public IEnumerable<string>? Suffixes { get; }

        /// <inheritdoc cref="WrapperExpression"/>
        /// <param name="prefixes"><inheritdoc cref="Prefixes"/></param>
        /// <param name="expression"><inheritdoc cref="Expression"/></param>
        /// <param name="suffixes"><inheritdoc cref="Suffixes"/></param>
        public WrapperExpression(IEnumerable<string>? prefixes, IExpression expression, IEnumerable<string>? suffixes)
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
