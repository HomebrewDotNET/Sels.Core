using Sels.Core.Data.SQL.Query;
using Sels.Core.Data.SQL.Query.Expressions;
using System.Text;

namespace Sels.Core.Data.MySQL.Query.Expressions
{
    /// <summary>
    /// Expression that represents the MySql CONCAT function for joining strings.
    /// </summary>
    public class ConcatExpression : BaseExpressionContainer
    {
        // Constants
        /// <summary>
        /// The name of the MySql Concat function.
        /// </summary>
        public const string Function = "CONCAT";

        // Properties
        /// <summary>
        /// The expressions to supply to the concat function.
        /// </summary>
        public IExpression[] Expressions { get; }

        /// <inheritdoc cref=" ConcatExpression"/>
        /// <param name="expressions"><inheritdoc cref="Expressions"/></param>
        public ConcatExpression(IEnumerable<IExpression> expressions)
        {
            expressions.ValidateArgumentNotNullOrEmpty(nameof(expressions));
            expressions.GetCount().ValidateArgumentLarger($"{nameof(expressions)}.Count()", 1);

            Expressions = expressions.ToArray();
        }
        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            builder.Append(Function).Append('(');
            Expressions.Execute((i, e) =>
            {
                subBuilder(builder, e);
                if(i < Expressions.Length-1) builder.Append(',');
            });
            builder.Append(')');
        }
    }
}
