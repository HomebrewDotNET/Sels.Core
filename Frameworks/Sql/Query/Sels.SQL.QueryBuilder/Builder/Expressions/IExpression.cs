using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Allows an object to be converted to sql.
    /// </summary>
    public interface IExpression
    {
        /// <summary>
        /// Converts the current expression to sql following the sql standard and appends to <paramref name="builder"/>. 
        /// Properties may need to be used instead based on the implementation (e.g. SqlLite, MsSql, MySql, ...)
        /// </summary>
        /// <param name="builder">The builder to append to</param>
        /// <param name="options">Optional settings for building the query</param>
        void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None);
    }
}
