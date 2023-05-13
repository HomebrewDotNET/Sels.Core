using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System.Text;

namespace Sels.SQL.QueryBuilder.MySQL.Expressions
{
    /// <summary>
    /// Expression that represents the limit keyword for limiting and optionally offsetting the rows returned during a select.
    /// </summary>
    public class LimitOffsetExpression : BaseExpressionContainer
    {
        // Constants
        /// <summary>
        /// The mysql limit keyword.
        /// </summary>
        public const string Keyword = "LIMIT";

        // Fields
        private IExpression _limitExpression;

        // Properties
        /// <summary>
        /// The expression containing the maximum amount of rows to return.
        /// </summary>
        public IExpression LimitExpression { get => _limitExpression; set => _limitExpression = value.ValidateArgument(nameof(LimitExpression)); }
        /// <summary>
        /// Optional expression containing the number of rows to skip.
        /// </summary>
        public IExpression? OffsetExpression { get; set; }

        ///<inheritdoc cref="LimitOffsetExpression"/>
        /// <param name="limitExpression"><inheritdoc cref="LimitExpression"/></param>
        /// <param name="offsetExpression"><inheritdoc cref="OffsetExpression"/></param>
        public LimitOffsetExpression(IExpression limitExpression, IExpression? offsetExpression)
        {
            LimitExpression = limitExpression.ValidateArgument(nameof(limitExpression));
            OffsetExpression = offsetExpression;
        }
        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            builder.Append(Keyword).AppendSpace();
            if(OffsetExpression != null){
                subBuilder(builder, OffsetExpression);
                builder.Append(Constants.Strings.Comma);
            }
            subBuilder(builder, LimitExpression);
        }
    }
}
