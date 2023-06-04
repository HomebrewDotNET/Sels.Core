using Sels.Core.Extensions;
using Sels.Core.Models;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Expressions
{
    /// <summary>
    /// Expression configured using a <see cref="ISharedExpressionBuilder{TEntity, TReturn}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the expression for</typeparam>
    public class ExpressionBuilder<TEntity> : BaseExpressionContainer, ISharedExpressionBuilder<TEntity, Null>
    {
        // Properties
        /// <summary>
        /// The expression created using the builder interface.
        /// </summary>
        public IExpression Expression { get; private set; }

        /// <inheritdoc cref="ExpressionBuilder{TEntity}"/>
        /// <param name="builder"></param>
        public ExpressionBuilder(Action<ISharedExpressionBuilder<TEntity, Null>> builder)
        {
            builder.ValidateArgument(nameof(builder))(this);
            if (Expression == null) throw new InvalidOperationException($"{nameof(builder)} did not create an expression");
        }

        /// <inheritdoc/>
        Null ISharedExpressionBuilder<TEntity, Null>.Expression(IExpression expression)
        {
            Expression = expression.ValidateArgument(nameof(expression));

            return Null.Value;
        }
        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            subBuilder(builder, Expression);
        }
    }
}
