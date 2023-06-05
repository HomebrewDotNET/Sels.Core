using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builder for declaring an SQL variable. Used to select the <see cref="IVariableDeclarationStatementBuilder.Variable"/> expression.
    /// </summary>
    public interface IVariableDeclarationRootStatementBuilder
    {
        /// <summary>
        /// Uses <paramref name="expression"/> as the variable name.
        /// </summary>
        /// <param name="expression">The expression to use</param>
        /// <returns>Current builder for method chaining</returns>
        IVariableDeclarationTypeStatementBuilder Variable(IExpression expression);
        /// <summary>
        /// Uses <paramref name="name"/> as the variable name.
        /// </summary>
        /// <param name="name">The name for the SQL variable</param>
        /// <returns>Current builder for method chaining</returns>
        IVariableDeclarationTypeStatementBuilder Variable(string name) => Variable(new VariableExpression(name.ValidateArgumentNotNullOrWhitespace(nameof(name))));
    }

    /// <summary>
    /// Builder for declaring an SQL variable. Used to select the <see cref="IVariableDeclarationStatementBuilder.Type"/> expression.
    /// </summary>
    public interface IVariableDeclarationTypeStatementBuilder
    {
        /// <summary>
        /// Uses <paramref name="expression"/> as the type for the SQL variable.
        /// </summary>
        /// <param name="expression">The expression to use</param>
        /// <returns>Current builder for method chaining</returns>
        IVariableDeclarationStatementBuilder As(IExpression expression);
        /// <summary>
        /// Uses <paramref name="rawSql"/> as the type for the SQL variable.
        /// </summary>
        /// <param name="rawSql">Raw SQL that defines the type for the SQL variable</param>
        /// <returns>Current builder for method chaining</returns>
        IVariableDeclarationStatementBuilder As(string rawSql) => As(new RawExpression(rawSql.ValidateArgumentNotNullOrWhitespace(nameof(rawSql))));
        /// <summary>
        /// Uses <typeparamref name="T"/> as the type for the sql variable.
        /// </summary>
        /// <typeparam name="T">The type to use</typeparam>
        /// <returns>Current builder for method chaining</returns>
        IVariableDeclarationStatementBuilder As<T>() => As(new TypeExpression(typeof(T)));
    }

    /// <summary>
    /// Builder for declaring an SQL variable.
    /// </summary>
    public interface IVariableDeclarationStatementBuilder : IQueryBuilder
    {
        /// <summary>
        /// Expression that contains the name of the variable to declare.
        /// </summary>
        IExpression Variable { get; }
        /// <summary>
        /// The type of the variable to declare.
        /// </summary>
        IExpression Type { get;  }
        /// <summary>
        /// Optional initial value for the variable. 
        /// </summary>
        IExpression InitialValue { get; }

        /// <summary>
        /// Returns a builder to set <see cref="InitialValue"/>.
        /// </summary>
        ISharedExpressionBuilder<object, IVariableDeclarationStatementBuilder> InitialzedBy { get; }
    }
}
