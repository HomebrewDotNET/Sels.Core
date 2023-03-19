using Sels.Core.Data.SQL.Query.Expressions;
using Sels.Core.Data.SQL.Query.Statement;
using System.Linq.Expressions;
using System.Text;

namespace Sels.Core.Data.MySQL.Query.Statement
{
    /// <summary>
    /// Builder for defining what to update on duplicate key during insert.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public interface IOnDuplicateKeyUpdateBuilder<TEntity> : IStatementSetBuilder<TEntity, IOnDuplicateKeyUpdateValueBuilder<TEntity>>
    {

    }
    /// <summary>
    /// Builder for defining what to set an expression to.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public interface IOnDuplicateKeyUpdateValueBuilder<TEntity> : ISharedExpressionBuilder<TEntity, IOnDuplicateKeyUpdateChainedBuilder<TEntity>>
    {
        /// <summary>
        /// Set an expression to a value supplied to the VALUES clause.
        /// </summary>
        /// <param name="expression">Expression pointing to the expression in the INSERT clause</param>
        /// <returns>Builder for setting more values</returns>
        IOnDuplicateKeyUpdateChainedBuilder<TEntity> Values(IExpression expression) => Expression(new WrapperExpression(new string[] { Sql.Clauses.Values, "(" }, expression.ValidateArgument(nameof(expression)), ")".AsArray()));
        /// <summary>
        /// Set an expression to a value supplied to the VALUES clause.
        /// </summary>
        /// <param name="expression">String containing the sql expression</param>
        /// <returns>Builder for setting more values</returns>
        IOnDuplicateKeyUpdateChainedBuilder<TEntity> Values(string expression) => Values(new RawExpression(expression.ValidateArgumentNotNullOrWhitespace(nameof(expression))));
        /// <summary>
        /// Set an expression to a value supplied to the VALUES clause.
        /// </summary>
        /// <param name="expression">Delegate that adds the sql expression to the provided string builder</param>
        /// <returns>Builder for setting more values</returns>
        IOnDuplicateKeyUpdateChainedBuilder<TEntity> Values(Action<StringBuilder> expression) => Values(new DelegateExpression(expression.ValidateArgument(nameof(expression))));
        /// <summary>
        /// Set an expression to a value supplied to the VALUES clause where the column name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to select the property from</typeparam>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for setting more values</returns>
        IOnDuplicateKeyUpdateChainedBuilder<TEntity> Values<T>(Expression<Func<T, object?>> property) => Values(new ColumnExpression(null, property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name));
        /// <summary>
        /// Set an expression to a value supplied to the VALUES clause where the column name is taken from the property name selected by <paramref name="property"/> from <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="property">The expression that points to the property to use</param>
        /// <returns>Builder for setting more values</returns>
        IOnDuplicateKeyUpdateChainedBuilder<TEntity> Values(Expression<Func<TEntity, object?>> property) => Values<TEntity>(property);
    }
    /// <summary>
    /// Builder for setting more values.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to create the query for</typeparam>
    public interface IOnDuplicateKeyUpdateChainedBuilder<TEntity>
    {
        /// <summary>
        /// Returns a builder for setting more values.
        /// </summary>
        IOnDuplicateKeyUpdateBuilder<TEntity> And { get; }
    }
}
