using Sels.Core.Extensions;
using Sels.Core.Extensions.Text;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.MySQL.Expressions
{
    /// <summary>
    /// Expression that represents <see cref="MySql.Statements.Signal.SqlState"/>.
    /// </summary>
    public class SqlStateExpression : BaseExpressionContainer
    {
        // Properties
        /// <summary>
        /// The string containing the sql state.
        /// </summary>
        public string State { get; }

        /// <inheritdoc cref="SqlStateExpression"/>
        /// <param name="state"><inheritdoc cref="State"/></param>
        public SqlStateExpression(string state)
        {
            State = state.ValidateArgumentNotNullOrWhitespace(nameof(state));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            builder.Append(MySql.Statements.Signal.SqlState).AppendSpace();
            subBuilder(builder, Sql.Expressions.Value(State));
        }
    }
}
