using Sels.Core.Extensions;
using System;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Expression that represents a table to perform a query on.
    /// </summary>
    public class TableExpression : BaseExpressionContainer
    {
        // Properties
        /// <inheritdoc cref="IDataSetExpression.DataSet"/>
        public DataSetExpression DataSet { get; set; }
        /// <summary>
        /// Contains the database where the table is located in.
        /// </summary>
        public ObjectExpression Database { get; set; }
        /// <summary>
        /// Contains the schema the table is located in.
        /// </summary>
        public ObjectExpression Schema { get; set; }
        /// <summary>
        /// Contains the table name.
        /// </summary>
        public ObjectExpression Table { get; }

        /// <inheritdoc cref="TableExpression"/>
        /// <param name="dataset"><inheritdoc cref="IDataSetExpression.DataSet"/></param>
        /// <param name="database"><inheritdoc cref="Database"/></param>
        /// <param name="schema"><inheritdoc cref="Schema"/></param>
        /// <param name="table"><inheritdoc cref="Table"/></param>
        public TableExpression(string database, string schema, string table, object dataset)
        {
            Table = new ObjectExpression(null, table.ValidateArgumentNotNullOrWhitespace(nameof(table)));
            DataSet = dataset != null ? new DataSetExpression(dataset) : null;
            Schema = schema != null ? new ObjectExpression(null, schema) : null;
            Database = database != null ? new ObjectExpression(null, database) : null;
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            if(Database != null)
            {
                subBuilder(builder, Database);
                builder.Append('.');
            }

            if (Schema != null)
            {
                subBuilder(builder, Schema);
                builder.Append('.');
            }

            subBuilder(builder, Table);

            if(DataSet != null)
            {
                builder.AppendSpace();
                subBuilder(builder, DataSet);
            }
        }
    }
}
