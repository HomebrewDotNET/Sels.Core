using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions.Text;
using System.Linq;

namespace Sels.SQL.QueryBuilder.MySQL.Expressions
{
    /// <summary>
    /// Expression that represents a <see cref="MySql.Statements.Signal"/> statement. 
    /// </summary>
    public class SignalStatementExpression : BaseExpressionContainer
    {
        // Properties
        /// <summary>
        /// Expression that contains the sql state that the signal statement should return.
        /// </summary>
        public IExpression SqlStateExpression { get; }
        /// <inheritdoc cref="SignalConditionInformation"/>
        public SignalConditionInformation ConditionInformation { get; } = new SignalConditionInformation();

        /// <inheritdoc cref="SignalStatementExpression"/>
        /// <param name="sqlStateExpression"><inheritdoc cref="SqlStateExpression"/></param>
        /// <param name="informationSetter">Optional delegate that can be used to configure <see cref="ConditionInformation"/></param>
        public SignalStatementExpression(IExpression sqlStateExpression, Action<SignalConditionInformation> informationSetter = null)
        {
            SqlStateExpression = sqlStateExpression.ValidateArgument(nameof(sqlStateExpression));

            informationSetter?.Invoke(ConditionInformation);
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            bool isFormatted = options.HasFlag(ExpressionCompileOptions.Format);

            builder.Append(MySql.Statements.Signal.Name).AppendSpace();
            subBuilder(builder, SqlStateExpression);

            var items = ConditionInformation.GetAssignedItems().ToArray();

            if (items.HasValue())
            {
                if (isFormatted)
                {
                    builder.AppendLine();
                }
                else
                {
                    builder.AppendSpace();
                }

                builder.Append(Sql.Set).AppendSpace();

                for(int i = 0; i < items.Length; i++)
                {
                    var (itemName, itemValue) = items[i];

                    builder.Append(itemName).AppendSpace().Append(Sql.AssignmentOperator).AppendSpace();
                    subBuilder(builder, Sql.Expressions.Value(itemValue));

                    if(i != (items.Length - 1))
                    {
                        builder.Append(',');
                        if (isFormatted)
                        {
                            builder.AppendLine();
                        }
                        else
                        {
                            builder.AppendSpace();
                        }
                    }
                }
            }

            if (options.HasFlag(ExpressionCompileOptions.AppendSeparator)) builder.Append(Sql.Separator);
        }
    }

    /// <summary>
    /// Additional diagnostics area condition information items that can be set in a <see cref="MySql.Statements.Signal"/> statement. 
    /// </summary>
    public class SignalConditionInformation
    {
        /// <summary>
        /// A string containing the class of the RETURNED_SQLSTATE value. If the RETURNED_SQLSTATE value begins with a class value defined in SQL standards document ISO 9075-2 (section 24.1, SQLSTATE), CLASS_ORIGIN is 'ISO 9075'. Otherwise, CLASS_ORIGIN is 'MySQL'.
        /// </summary>
        public string ClassOrigin { get; set; }
        /// <summary>
        /// A string containing the subclass of the RETURNED_SQLSTATE value. If CLASS_ORIGIN is 'ISO 9075' or RETURNED_SQLSTATE ends with '000', SUBCLASS_ORIGIN is 'ISO 9075'. Otherwise, SUBCLASS_ORIGIN is 'MySQL'.
        /// </summary>
        public string SubClassOrigin { get; set; }
        /// <summary>
        /// Strings that indicate the catalog for a violated constraint.
        /// </summary>
        public string ConstraintCatalog { get; set; }
        /// <summary>
        /// Strings that indicate the schema for a violated constraint.
        /// </summary>
        public string ConstraintSchema { get; set; }
        /// <summary>
        /// Strings that indicate the name for a violated constraint.
        /// </summary>
        public string ConstraintName { get; set; }
        /// <summary>
        /// Strings that indicate the catalog related to the condition.
        /// </summary>
        public string CatalogName { get; set; }
        /// <summary>
        /// Strings that indicate the schema related to the condition.
        /// </summary>
        public string SchemaName { get; set; }
        /// <summary>
        /// Strings that indicate the table related to the condition.
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// Strings that indicate the column related to the condition.
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// A string that indicates the cursor name.
        /// </summary>
        public string CursorName { get; set; }
        /// <summary>
        /// A string that indicates the error message for the condition.
        /// </summary>
        public string MessageText { get; set; }
        /// <summary>
        /// An integer that indicates the MySQL error code for the condition
        /// </summary>
        public ushort? MySqlErrorNumber { get; set; }

        /// <summary>
        /// Returns an enumerator that returns all assigned (not null) values and the sql name.
        /// </summary>
        /// <returns>All assigned (not null) values and the sql name. ItemName: The sql name of the item, Value: The value assigned to the item</returns>
        public IEnumerable<(string ItemName, object Value)> GetAssignedItems()
        {
            if (ClassOrigin.HasValue()) yield return ("CLASS_ORIGIN", ClassOrigin);
            if (SubClassOrigin.HasValue()) yield return ("SUBCLASS_ORIGIN", SubClassOrigin);
            if (ConstraintCatalog.HasValue()) yield return ("CONSTRAINT_CATALOG", ConstraintCatalog);
            if (ConstraintSchema.HasValue()) yield return ("CONSTRAINT_SCHEMA", ConstraintSchema);
            if (ConstraintName.HasValue()) yield return ("CONSTRAINT_NAME", ConstraintName);
            if (CatalogName.HasValue()) yield return ("CATALOG_NAME", CatalogName);
            if (SchemaName.HasValue()) yield return ("SCHEMA_NAME", SchemaName);
            if (TableName.HasValue()) yield return ("TABLE_NAME", TableName);
            if (ColumnName.HasValue()) yield return ("COLUMN_NAME", ColumnName);
            if (CursorName.HasValue()) yield return ("CURSOR_NAME", CursorName);
            if (MessageText.HasValue()) yield return ("MESSAGE_TEXT", MessageText);
            if (MySqlErrorNumber.HasValue) yield return ("MYSQL_ERRNO", MySqlErrorNumber.Value);
        }
    }
}
