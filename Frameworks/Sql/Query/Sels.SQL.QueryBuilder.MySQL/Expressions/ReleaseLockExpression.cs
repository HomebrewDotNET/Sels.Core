using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.MySQL.Expressions
{
    /// <summary>
    /// Expression that calls the <see cref="MySql.Functions.ReleaseLock"/> function.
    /// </summary>
    public class ReleaseLockExpression : BaseExpressionContainer
    {
        // Proeprties
        /// <summary>
        /// Expression that contains the identifier to release the lock from.
        /// </summary>
        public IExpression IdentifierExpression { get; }

        /// <inheritdoc cref="ReleaseLockExpression"/>
        /// <param name="identifierExpression"><inheritdoc cref="IdentifierExpression"/></param>
        public ReleaseLockExpression(IExpression identifierExpression)
        {
            IdentifierExpression = identifierExpression.ValidateArgument(nameof(identifierExpression));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            builder.Append(MySql.Functions.ReleaseLock).Append('(');
            subBuilder(builder, IdentifierExpression);
            builder.Append(')');
        }
    }
}
