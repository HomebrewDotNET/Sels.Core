using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Expressions
{
    /// <summary>
    /// Expression that represents a variable that's being assigned a new value. For example in WHERE or SELECT statements.
    /// </summary>
    public class VariableInlineAssignmentExpression : BaseExpressionContainer
    {
        // Properties
        /// <summary>
        /// Expression that contains the variable to set.
        /// </summary>
        public IExpression VariableExpression { get; }
        /// <summary>
        /// The assignment operator to use.
        /// </summary>
        public string AssignmentOperator { get; set; } = Sql.AssignmentOperator.ToString();
        /// <summary>
        /// Expression that contains the value to set <see cref="VariableExpression"/> to.
        /// </summary>
        public IExpression ValueExpression { get; }

        /// <inheritdoc/>
        /// <param name="variableExpression"><inheritdoc cref="VariableExpression"/></param>
        /// <param name="valueExpression"><inheritdoc cref="ValueExpression"/></param>
        public VariableInlineAssignmentExpression(IExpression variableExpression, IExpression valueExpression)
        {
            VariableExpression = variableExpression.ValidateArgument(nameof(variableExpression));
            ValueExpression = valueExpression.ValidateArgument(nameof(valueExpression));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            subBuilder(builder, VariableExpression);
            builder.AppendSpace().Append(AssignmentOperator.ValidateArgumentNotNullOrWhitespace(nameof(AssignmentOperator))).AppendSpace();
            subBuilder(builder, ValueExpression);
        }
    }
}
