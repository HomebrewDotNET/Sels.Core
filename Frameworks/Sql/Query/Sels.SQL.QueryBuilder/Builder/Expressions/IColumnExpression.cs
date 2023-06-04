using System;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that contains sql columns that can be converted to sql strings.
    /// </summary>
    public interface IColumnExpression : IObjectExpression
    {
        // Properties
        /// <summary>
        /// Optional alias for <see cref="IObjectExpression.Object"/>.
        /// </summary>
        public string? Alias { get; }

        /// <summary>
        /// Converts the current expression to sql.
        /// </summary>
        /// <param name="builder">The builder to append to</param>
        /// <param name="datasetConverterer">Delegate for converting the column dataset to sql</param>
        /// <param name="columnConverter">Delegate for converting the column name to sql</param>
        /// <param name="includeAlias">If the column alias should also be appended to <paramref name="builder"/></param>
        /// <param name="options">Optional settings for building the query</param>
        public void ToSql(StringBuilder builder, Func<object, string?> datasetConverterer, Func<string, string>? columnConverter, bool includeAlias = true, ExpressionCompileOptions options = ExpressionCompileOptions.None);
    }
}
