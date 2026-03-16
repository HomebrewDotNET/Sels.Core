using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using Sels.SQL.QueryBuilder.Templates.Query.Expressions.Proxy;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions.Text;

namespace Sels.SQL.QueryBuilder.Expressions.Select
{
    /// <summary>
    /// Represents a value returned from a select statement.
    /// </summary>
    /// <typeparam name="TDerived">The type to return for the fluent syntax</typeparam>
    /// <typeparam name="TEntity">The main entity to select from</typeparam>
    public class SelectedValueExpression<TEntity, TDerived> : BaseDelegateSelectStatementBuilder<TEntity, TDerived>, 
        IExpressionContainer,
        ISelectStatementSelectedValueBuilder<TEntity, TDerived>
    {
        // Properties
        /// <summary>
        /// Optional alias for the selected value.
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// The expression that contains the selected value.
        /// </summary>
        public IExpression ValueExpression { get; }

        /// <inheritdoc cref="SelectedValueExpression{TEntity, TDerived}"/>
        /// <param name="expression">Expression that contains the selected value</param>
        /// <param name="target">The builder the selected value is attached to</param>
        public SelectedValueExpression(IExpression expression, ISelectStatementBuilder<TEntity, TDerived> target) : base(target)
        {
            ValueExpression = expression.ValidateArgument(nameof(expression));
        }
        /// <inheritdoc/>
        public TDerived As(string alias)
        {
            Alias = alias.ValidateArgumentNotNullOrWhitespace(nameof(alias));
            return Instance;
        }

        /// <inheritdoc/>
        public virtual void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            subBuilder(builder, ValueExpression);
            if (Alias.HasValue())
            {
                builder.AppendSpace().Append(Sql.As).AppendSpace();
                subBuilder(builder, new ObjectExpression(null, Alias));
            }
        }

        /// <inheritdoc/>
        public void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            ToSql(builder, (b, e) => e.ToSql(b), options);
        }
    }
}
