using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions;
using Sels.Core.Models;
using Sels.SQL.QueryBuilder.Expressions;


namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builder for creating the conditions for an IF or ELSE IF statement.
    /// </summary>
    public interface IIfConditionStatementBuilder
    {
        /// <summary>
        /// Adds <paramref name="expression"/> as a condition expression.
        /// </summary>
        /// <param name="expression">The expression to add as a condition</param>
        /// <returns>Current builder for method chaining</returns>
        IIfConditionOrBodyStatementBuilder Condition(IExpression expression);
        /// <summary>
        /// Adds <paramref name="rawSql"/> as a SQL condition.
        /// </summary>
        /// <param name="rawSql">String that contains raw sql</param>
        /// <returns>Current builder for method chaining</returns>
        IIfConditionOrBodyStatementBuilder Condition(string rawSql) => Condition(new RawExpression(rawSql.ValidateArgumentNotNullOrWhitespace(nameof(rawSql))));
        /// <summary>
        /// Creates a condition using <paramref name="builder"/>.
        /// </summary>
        /// <typeparam name="TEntity">The main entity to create the condition for</typeparam>
        /// <param name="builder">The delegate used to configure the condition</param>
        /// <returns>Current builder for method chaining</returns>
        IIfConditionOrBodyStatementBuilder Condition<TEntity>(Func<IStatementConditionExpressionBuilder<TEntity>, IChainedBuilder<TEntity, IStatementConditionExpressionBuilder<TEntity>>> builder) => Condition(new ConditionGroupExpression<TEntity>(builder, false, true));
        /// <summary>
        /// Creates a condition using <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The delegate used to configure the condition</param>
        /// <returns>Current builder for method chaining</returns>
        IIfConditionOrBodyStatementBuilder Condition(Func<IStatementConditionExpressionBuilder<object>, IChainedBuilder<object, IStatementConditionExpressionBuilder<object>>> builder) => Condition<object>(builder);
    }

    /// <summary>
    /// Builder for creating additional conditions for an IF or ELSE IF statement and for defining the body of an IF or ELSE IF statement.
    /// </summary>
    public interface IIfConditionOrBodyStatementBuilder : IIfConditionStatementBuilder, IIfBodyStatementBuilder
    {

    }

    /// <summary>
    /// Builder for defining the body of an IF, ELSE IF or  statement.
    /// </summary>
    public interface IIfBodyStatementBuilder
    {
        /// <summary>
        /// Defines the SQL to execute when the previously defined condition is evaluted to true using <paramref name="builderAction"/>.
        /// </summary>
        /// <param name="builderAction">Delegate for adding the SQL to execute</param>
        /// <returns>Current builder for method chaining</returns>
        IIfFullStatementBuilder Then(Action<IMultiStatementBuilder> builderAction);
        /// <summary>
        /// Adds <paramref name="expression"/> as the SQL to execute when the previously defined condition is evaluted to true.
        /// </summary>
        /// <param name="expression">The expression to add</param>
        /// <param name="isFullStatement">Whether or not <paramref name="expression"/> compiles to a full sql statement. Is used to handle compilation options like <see cref="ExpressionCompileOptions.AppendSeparator"/> and <see cref="ExpressionCompileOptions.Format"/></param>
        /// <returns>Current builder for method chaining</returns>
        IIfFullStatementBuilder Then(IExpression expression, bool isFullStatement = true) => Then(b => b.Append(expression.ValidateArgument(nameof(expression)), isFullStatement));
        /// <summary>
        /// Adds the expression created by <paramref name="expressionBuilder"/> so it can be compiled into sql.
        /// </summary>
        /// <param name="expressionBuilder">Delegate used to configure the expression to add</param>
        /// <param name="isFullStatement">Whether or not the expression created by <paramref name="expressionBuilder"/> compiles to a full sql statement. Is used to handle compilation options like <see cref="ExpressionCompileOptions.AppendSeparator"/> and <see cref="ExpressionCompileOptions.Format"/></param>
        /// <returns>Current builder for method chaining</returns>
        IIfFullStatementBuilder Then<TEntity>(Action<ISharedExpressionBuilder<TEntity, Null>> expressionBuilder, bool isFullStatement = true) => Then(new ExpressionBuilder<TEntity>(expressionBuilder.ValidateArgument(nameof(expressionBuilder))), isFullStatement);
        /// <summary>
        /// Adds the expression created by <paramref name="expressionBuilder"/> so it can be compiled into sql.
        /// </summary>
        /// <param name="expressionBuilder">Delegate used to configure the expression to add</param>
        /// <param name="isFullStatement">Whether or not the expression created by <paramref name="expressionBuilder"/> compiles to a full sql statement. Is used to handle compilation options like <see cref="ExpressionCompileOptions.AppendSeparator"/> and <see cref="ExpressionCompileOptions.Format"/></param>
        /// <returns>Current builder for method chaining</returns>
        IIfFullStatementBuilder Then(Action<ISharedExpressionBuilder<Null, Null>> expressionBuilder, bool isFullStatement = true) => Then<Null>(expressionBuilder, isFullStatement);
        /// <summary>
        /// Adds <paramref name="rawSql"/> as the raw SQL to execute when the previously defined condition is evaluted to true.
        /// </summary>
        /// <param name="rawSql">String containing raw sql</param>
        /// <param name="isFullStatement">Whether or not <paramref name="rawSql"/> compiles to a full sql statement. Is used to handle compilation options like <see cref="ExpressionCompileOptions.AppendSeparator"/> and <see cref="ExpressionCompileOptions.Format"/></param>
        /// <returns>Current builder for method chaining</returns>
        IIfFullStatementBuilder Then(string rawSql, bool isFullStatement = true) => Then(b => b.Append(rawSql.ValidateArgumentNotNullOrWhitespace(nameof(rawSql)), isFullStatement));
        /// <summary>
        /// Compiles <paramref name="queryBuilder"/> into the SQL to execute when the previously defined condition is evaluted to true.
        /// </summary>
        /// <param name="queryBuilder">The builder to add</param>
        /// <returns>Current builder for method chaining</returns>
        IIfFullStatementBuilder Then(IQueryBuilder queryBuilder) => Then(b => b.Append(queryBuilder.ValidateArgument(nameof(queryBuilder))));
    }

    /// <summary>
    /// Builder for creating IF ELSE conditions in SQL.
    /// </summary>
    public interface IIfFullStatementBuilder : IIfBodyStatementBuilder, IIfStatementBuilder
    {
        /// <summary>
        /// Returns a builder for creating an ELSE IF statement.
        /// </summary>
        public IIfConditionOrBodyStatementBuilder ElseIf { get; }
        /// <summary>
        /// Returns a builder for creating the body for the ELSE statement.
        /// </summary>
        public IIfBodyStatementBuilder Else { get; }
    }

    /// <summary>
    /// Builder for creating IF ELSE conditions in SQL.
    /// </summary>
    public interface IIfStatementBuilder : IQueryBuilder
    {
        // Properties
        /// <summary>
        /// The conditions for the IF statement.
        /// </summary>
        public IExpression[] ConditionExpressions { get; } 
        /// <summary>
        /// The multi statement builder for creating the body for the IF statement.
        /// </summary>
        public IMultiStatementBuilder BodyBuilder { get; }
        /// <summary>
        /// The else if statements for the condition builder. The key are the conditions for the ELSE IF statement and the value is the builder used to created the body for the ELSE IF statement. When dictionary is empty no ELSE IF statements will be created.
        /// </summary>
        public IReadOnlyDictionary<IExpression[], IMultiStatementBuilder> ElseIfStatements { get; }
        /// <summary>
        /// The multi statement builder for creating the body for the ELSE statement. When builder is empty no ELSE statement will be created.
        /// </summary>
        public IMultiStatementBuilder ElseBodyBuilder { get; }
    }
}
