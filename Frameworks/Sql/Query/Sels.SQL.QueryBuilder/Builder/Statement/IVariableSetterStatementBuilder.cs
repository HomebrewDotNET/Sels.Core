using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builder for setting the value of a SQL variable. Used to select the <see cref="IVariableSetterStatementBuilder.Variable"/> expression.
    /// </summary>
    public interface IVariableSetterRootStatementBuilder
    {
        /// <summary>
        /// Uses <paramref name="expression"/> as the variable name.
        /// </summary>
        /// <param name="expression">The expression to use</param>
        /// <returns>Current builder for method chaining</returns>
        IVariableSetterValueStatementBuilder Variable(IExpression expression);
        /// <summary>
        /// Uses <paramref name="name"/> as the variable name.
        /// </summary>
        /// <param name="name">The name for the SQL variable</param>
        /// <returns>Current builder for method chaining</returns>
        IVariableSetterValueStatementBuilder Variable(string name) => Variable(new VariableExpression(name.ValidateArgumentNotNullOrWhitespace(nameof(name))));
    }

    /// <summary>
    /// Builder for setting the value of a SQL variable. Used to select the <see cref="IVariableSetterStatementBuilder.Value"/> expression.
    /// </summary>
    public interface IVariableSetterValueStatementBuilder
    {
        /// <summary>
        /// Returns a builder to set <see cref="IVariableSetterStatementBuilder.Value"/>
        /// </summary>
        ISharedExpressionBuilder<object, IVariableSetterStatementBuilder> To { get; }
    }

    /// <summary>
    /// Builder for setting the value of a SQL variable.
    /// </summary>
    public interface IVariableSetterStatementBuilder : IQueryBuilder
    {
        /// <summary>
        /// The name of the variable to set.
        /// </summary>
        IExpression Variable { get; }
        /// <summary>
        /// The value to assign to the variable.
        /// </summary>
        IExpression Value { get; }
    }
}
