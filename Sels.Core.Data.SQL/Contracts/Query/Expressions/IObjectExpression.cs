using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that contains an sql object (Column, table, ...).
    /// </summary>
    public interface IObjectExpression : IDataSetExpression
    {
        /// <summary>
        /// The name of the sql object.
        /// </summary>
        string Object { get; }

        /// <summary>
        /// Converts the current expression to sql.
        /// </summary>
        /// <param name="builder">The builder to append to</param>
        /// <param name="datasetConverterer">Delegate for converting the column dataset to sql</param>
        /// <param name="objectConverter">Delegate for converting the object name to sql</param>
        /// <param name="options">Optional settings for building the query</param>
        public void ToSql(StringBuilder builder, Func<object, string?> datasetConverterer, Func<string, string>? objectConverter, QueryBuilderOptions options = QueryBuilderOptions.None);
    }
}
