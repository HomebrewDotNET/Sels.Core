using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.MySQL.Expressions
{
    /// <summary>
    /// Expression that calls the <see cref="MySql.Functions.GetLock"/> function.
    /// </summary>
    public class GetLockExpression : BaseExpressionContainer
    {
        // Proeprties
        /// <summary>
        /// Expression that contains the identifier to place the lock on.
        /// </summary>
        public IExpression IdentifierExpression { get; }
        /// <summary>
        /// Expression that contains the timeout value in seconds.
        /// </summary>
        public IExpression TimeoutExpression { get; }

        /// <inheritdoc cref="GetLockExpression"/>
        /// <param name="identifierExpression"><inheritdoc cref="IdentifierExpression"/></param>
        /// <param name="timeoutExpression"><inheritdoc cref="TimeoutExpression"/></param>
        public GetLockExpression(IExpression identifierExpression, IExpression timeoutExpression)
        {
            IdentifierExpression = identifierExpression.ValidateArgument(nameof(identifierExpression));
            TimeoutExpression = timeoutExpression.ValidateArgument(nameof(timeoutExpression));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            builder.Append(MySql.Functions.GetLock).Append('(');
            subBuilder(builder, IdentifierExpression);
            builder.Append(", ");
            subBuilder(builder, TimeoutExpression);
            builder.Append(')');
        }
    }
}
