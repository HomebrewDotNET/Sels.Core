using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.SQL.QueryBuilder.Expressions
{
    /// <summary>
    /// Expression that represents a sql function with optionally arguments for the function.
    /// </summary>
    public class FunctionExpression : BaseExpressionContainer
    {
        // Properties
        /// <summary>
        /// Expression that contains the function name.
        /// </summary>
        public IExpression FunctionNameExpression { get; }
        /// <summary>
        /// Optional expressions that contain the arguments for the function.
        /// </summary>
        public IExpression[] ArgumentExpressions { get; }

        /// <inheritdoc cref="FunctionExpression"/>
        /// <param name="functionNameExpression"><inheritdoc cref="FunctionNameExpression"/></param>
        /// <param name="argumentExpressions"><inheritdoc cref="ArgumentExpressions"/></param>
        public FunctionExpression(IExpression functionNameExpression, IEnumerable<IExpression> argumentExpressions)
        {
            FunctionNameExpression = functionNameExpression.ValidateArgument(nameof(functionNameExpression));
            ArgumentExpressions = argumentExpressions.ToArrayOrDefault();
        }
        /// <inheritdoc cref="FunctionExpression"/>
        /// <param name="functionNameExpression"><inheritdoc cref="FunctionNameExpression"/></param>
        /// <param name="argumentExpressions"><inheritdoc cref="ArgumentExpressions"/></param>
        public FunctionExpression(IExpression functionNameExpression, params IExpression[] argumentExpressions) : this(functionNameExpression, argumentExpressions.CastToOrDefault<IEnumerable<IExpression>>())
        {
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            // Function name
            subBuilder(builder, FunctionNameExpression);
            builder.Append('(');

            // Arguments
            if (ArgumentExpressions.HasValue())
            {
                ArgumentExpressions.Execute((i, e) =>
                {
                    subBuilder(builder, e);
                    if (i < ArgumentExpressions.Length - 1) builder.Append(", ");
                });
            }
            builder.Append(')');
        }
    }
}
