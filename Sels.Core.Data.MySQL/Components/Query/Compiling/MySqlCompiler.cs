using Microsoft.Extensions.Logging;
using Sels.Core.Data.SQL.Query;
using Sels.Core.Data.SQL.Query.Compilation;
using Sels.Core.Data.SQL.Query.Expressions;
using Sels.Core.Data.SQL.Query.Statement;
using System.Text;

namespace Sels.Core.Data.MySQL.Query.Compiling
{
    /// <summary>
    /// Query compiler that converts sql expressions to the mysql syntax.
    /// </summary>
    public class MySqlCompiler :
        IQueryCompiler<InsertExpressionPositions>,
        IQueryCompiler<SelectExpressionPositions>,
        IQueryCompiler<UpdateExpressionPositions>,
        IQueryCompiler<DeleteExpressionPositions>,
        IExpressionCompiler
    {
        // Fields
        private readonly IEnumerable<ILogger>? _loggers;
        private readonly string _name = nameof(MySqlCompiler);

        #region Configs
        private static readonly IReadOnlyDictionary<InsertExpressionPositions, (bool IsNewLine, string? Prefix, string[]? ExpressionJoinValues, string? Suffix, bool IsSingleExpression)> _insertPositionConfigs = new Dictionary<InsertExpressionPositions, (bool IsNewLine, string? Prefix, string[]? ExpressionJoinValues, string? Suffix, bool IsSingleExpression)>()
        {
            { InsertExpressionPositions.Into, (false, Sql.Clauses.Into, null, null, true) },
            { InsertExpressionPositions.Columns, (false, "(", new string[] {  Constants.Strings.Comma}, ")", false) },
            { InsertExpressionPositions.Values, (true, Sql.Clauses.Values + Constants.Strings.Space, new string[]{ Constants.Strings.Comma , Environment.NewLine}, null, false) }
        };
        private static readonly IReadOnlyDictionary<SelectExpressionPositions, (bool IsNewLine, string? Clause, string? ExpressionJoinValue, bool IsSingleExpression)> _selectPositionConfigs = new Dictionary<SelectExpressionPositions, (bool IsNewLine, string? Clause, string? ExpressionJoinValue, bool IsSingleExpression)>()
        {
            { SelectExpressionPositions.Column, (false, null, Constants.Strings.Comma, false) },
            { SelectExpressionPositions.From, (true, Sql.Clauses.From, null, true) },
            { SelectExpressionPositions.Join, (true, null, Environment.NewLine, false) },
            { SelectExpressionPositions.Where, (true, Sql.Clauses.Where, Constants.Strings.Comma + Sql.LogicOperators.And, false) },
            { SelectExpressionPositions.OrderBy, (true, Sql.Clauses.OrderBy, Constants.Strings.Comma, false) },
            { SelectExpressionPositions.GroupBy, (true, Sql.Clauses.GroupBy, Constants.Strings.Comma, false) },
            { SelectExpressionPositions.After, (true, null, Environment.NewLine, false) },
        };
        private static readonly IReadOnlyDictionary<UpdateExpressionPositions, (bool IsNewLine, string? Prefix, string[]? ExpressionJoinValues, string? Suffix, bool IsSingleExpression)> _updatePositionConfigs = new Dictionary<UpdateExpressionPositions, (bool IsNewLine, string? Prefix, string[]? ExpressionJoinValues, string? Suffix, bool IsSingleExpression)>()
        {
            { UpdateExpressionPositions.Table, (false, null, new string[] {  Constants.Strings.Comma}, null, false) },
            { UpdateExpressionPositions.Join, (true, null, new string[] {  Environment.NewLine}, null, false) },
            { UpdateExpressionPositions.Set, (true, Sql.Clauses.Set + Constants.Strings.Space, new string[] {  Environment.NewLine , Constants.Strings.Comma }, null, false) },
            { UpdateExpressionPositions.Where, (true, Sql.Clauses.Where + Constants.Strings.Space, new string[] { Constants.Strings.Comma, Sql.LogicOperators.And }, null, false) }
        };
        private static readonly IReadOnlyDictionary<DeleteExpressionPositions, (bool IsNewLine, string? Clause, string? ExpressionJoinValue, bool IsSingleExpression)> _deletePositionConfigs = new Dictionary<DeleteExpressionPositions, (bool IsNewLine, string? Clause, string? ExpressionJoinValue, bool IsSingleExpression)>()
        {
            { DeleteExpressionPositions.From, (true, Sql.Clauses.From, Constants.Strings.Comma, false) },
            { DeleteExpressionPositions.Join, (true, null, Environment.NewLine, false) },
            { DeleteExpressionPositions.Where, (true, Sql.Clauses.Where, Constants.Strings.Comma + Sql.LogicOperators.And, false) },
            { DeleteExpressionPositions.After, (true, null, Environment.NewLine, false) },
        };
        #endregion

        /// <inheritdoc cref="MySqlCompiler"/>
        /// <param name="loggers"></param>
        public MySqlCompiler(IEnumerable<ILogger>? loggers = null)
        {
            _loggers = loggers;
        }

        #region Insert
        /// <inheritdoc/>
        void IQueryCompiler<InsertExpressionPositions>.CompileTo(StringBuilder builder, IQueryBuilder<InsertExpressionPositions> queryBuilder, Func<object, string?>? datasetConverterer, ExpressionCompileOptions options)
        {
            using (_loggers.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                queryBuilder.ValidateArgument(nameof(queryBuilder));
                var builderExpressions = queryBuilder.Expressions;

                var expressionCount = builderExpressions.SelectMany(x => x.Value).GetCount();
                var isFormatted = options.HasFlag(ExpressionCompileOptions.Format);

                _loggers.Log($"{_name} compiling <{expressionCount}> expressions into a insert query");
                if (expressionCount == 0) throw new NotSupportedException($"No expressions to compile into a insert query");

                builder.Append(Sql.Statements.Insert);

                foreach (var builderExpression in builderExpressions.OrderBy(x => x.Key).Where(x => x.Value.HasValue()))
                {
                    var position = builderExpression.Key;
                    var expressions = builderExpression.Value;

                    // Position settings
                    var isConfigSet = _insertPositionConfigs.ContainsKey(position);
                    var isNewLine = isConfigSet ? _insertPositionConfigs[position].IsNewLine : false;
                    var prefix = isConfigSet ? _insertPositionConfigs[position].Prefix : null;
                    var joinValues = (isConfigSet ? _insertPositionConfigs[position].ExpressionJoinValues : null) ?? Constants.Strings.Space.AsArray();
                    var suffix = isConfigSet ? _insertPositionConfigs[position].Suffix : null;
                    var isSingleExpression = isConfigSet ? _insertPositionConfigs[position].IsSingleExpression : false;

                    _loggers.Debug($"{_name} compiling <{expressions.Length}> expressions for position <{position}>");

                    // Formatting
                    if (isFormatted && isNewLine)
                    {
                        builder.AppendLine();
                    }
                    else
                    {
                        builder.AppendSpace();
                    }

                    // Prefix
                    if (prefix.HasValue())
                    {
                        builder.Append(prefix).AppendSpace();
                    }

                    // Single value
                    if (isSingleExpression)
                    {
                        if (expressions.Length > 1) _loggers.Warning($"Expected only 1 expression for position <{position}> but got <{expressions.Length}>. Taking the last expression");
                        CompileExpressionTo(builder, datasetConverterer, options, expressions.Last(), true);
                    }
                    else
                    {
                        expressions.Execute((i, e) =>
                        {

                            CompileExpressionTo(builder, datasetConverterer, options, e, true);
                            if (i < expressions.Length - 1) {
                                var joinChars = joinValues.Select(x => x == Environment.NewLine && !isFormatted ? Constants.Strings.Space : x);
                                joinChars.Execute(x => builder.Append(x));
                            }
                        });
                    }

                    // Suffix
                    if (suffix.HasValue())
                    {
                        builder.Append(suffix);
                    }
                }
            }
        }
        #endregion

        #region Select
        /// <inheritdoc/>
        void IQueryCompiler<SelectExpressionPositions>.CompileTo(StringBuilder builder, IQueryBuilder<SelectExpressionPositions> queryBuilder, Func<object, string?>? datasetConverterer, ExpressionCompileOptions options)
        {
            using (_loggers.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                queryBuilder.ValidateArgument(nameof(queryBuilder));
                var builderExpressions = queryBuilder.Expressions;

                var expressionCount = builderExpressions.SelectMany(x => x.Value).GetCount();
                var isFormatted = options.HasFlag(ExpressionCompileOptions.Format);

                _loggers.Log($"{_name} compiling <{expressionCount}> expressions into a select query");
                if (expressionCount == 0) throw new NotSupportedException($"No expressions to compile into a select query");
 
                builder.Append(Sql.Statements.Select);

                foreach (var builderExpression in builderExpressions.OrderBy(x => x.Key).Where(x => x.Value.HasValue()))
                {
                    var position = builderExpression.Key;
                    var expressions = builderExpression.Value;

                    // Position settings
                    var isConfigSet = _selectPositionConfigs.ContainsKey(position);
                    var isNewLine = isConfigSet ? _selectPositionConfigs[position].IsNewLine : false;
                    var clause = isConfigSet ? _selectPositionConfigs[position].Clause : null;
                    var joinValue = (isConfigSet ? _selectPositionConfigs[position].ExpressionJoinValue : null) ?? Constants.Strings.Space;
                    var isSingleExpression = isConfigSet ? _selectPositionConfigs[position].IsSingleExpression : false;

                    _loggers.Debug($"{_name} compiling <{expressions.Length}> expressions for position <{position}>");

                    // Formatting
                    if (isFormatted && isNewLine)
                    {
                        builder.AppendLine();
                    }
                    else
                    {
                        builder.AppendSpace();
                    }

                    //Clause
                    if (clause.HasValue())
                    {
                        builder.Append(clause).AppendSpace();
                    }

                    // Single value
                    if (isSingleExpression)
                    {
                        if (expressions.Length > 1) _loggers.Warning($"Expected only 1 expression for position <{position}> but got <{expressions.Length}>. Taking the last expression");
                        CompileExpressionTo(builder, datasetConverterer, options, expressions.Last());
                        continue;
                    }
                    else
                    {
                        expressions.Execute((i, e) =>
                        {
                            var expressionJoinValue = joinValue == Environment.NewLine && !isFormatted ? Constants.Strings.Space : joinValue;

                            CompileExpressionTo(builder, datasetConverterer, options, e);
                            if (i < expressions.Length - 1) builder.Append(expressionJoinValue);
                        });
                    }
                }
            }
        }
        #endregion

        #region Update
        /// <inheritdoc/>
        public void CompileTo(StringBuilder builder, IQueryBuilder<UpdateExpressionPositions> queryBuilder, Func<object, string?>? datasetConverterer, ExpressionCompileOptions options)
        {
            using (_loggers.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                queryBuilder.ValidateArgument(nameof(queryBuilder));
                var builderExpressions = queryBuilder.Expressions;

                var expressionCount = builderExpressions.SelectMany(x => x.Value).GetCount();
                var isFormatted = options.HasFlag(ExpressionCompileOptions.Format);

                _loggers.Log($"{_name} compiling <{expressionCount}> expressions into an update query");
                if (expressionCount == 0) throw new NotSupportedException($"No expressions to compile into an update query");

                builder.Append(Sql.Statements.Update);

                foreach (var builderExpression in builderExpressions.OrderBy(x => x.Key).Where(x => x.Value.HasValue()))
                {
                    var position = builderExpression.Key;
                    var expressions = builderExpression.Value;

                    // Position settings
                    var isConfigSet = _updatePositionConfigs.ContainsKey(position);
                    var isNewLine = isConfigSet ? _updatePositionConfigs[position].IsNewLine : false;
                    var prefix = isConfigSet ? _updatePositionConfigs[position].Prefix : null;
                    var joinValues = (isConfigSet ? _updatePositionConfigs[position].ExpressionJoinValues : null) ?? Constants.Strings.Space.AsArray();
                    var suffix = isConfigSet ? _updatePositionConfigs[position].Suffix : null;
                    var isSingleExpression = isConfigSet ? _updatePositionConfigs[position].IsSingleExpression : false;

                    _loggers.Debug($"{_name} compiling <{expressions.Length}> expressions for position <{position}>");

                    // Formatting
                    if (isFormatted && isNewLine)
                    {
                        builder.AppendLine();
                    }
                    else
                    {
                        builder.AppendSpace();
                    }

                    // Prefix
                    if (prefix.HasValue())
                    {
                        builder.Append(prefix).AppendSpace();
                    }

                    // Single value
                    if (isSingleExpression)
                    {
                        if (expressions.Length > 1) _loggers.Warning($"Expected only 1 expression for position <{position}> but got <{expressions.Length}>. Taking the last expression");
                        CompileExpressionTo(builder, datasetConverterer, options, expressions.Last());
                    }
                    else
                    {
                        expressions.Execute((i, e) =>
                        {

                            CompileExpressionTo(builder, datasetConverterer, options, e);
                            if (i < expressions.Length - 1)
                            {
                                var joinChars = joinValues.Select(x => x == Environment.NewLine && !isFormatted ? Constants.Strings.Space : x);
                                joinChars.Execute(x => builder.Append(x));
                            }
                        });
                    }

                    // Suffix
                    if (suffix.HasValue())
                    {
                        builder.Append(suffix);
                    }
                }
            }
        }
        #endregion

        #region Delete
        /// <inheritdoc/>
        void IQueryCompiler<DeleteExpressionPositions>.CompileTo(StringBuilder builder, IQueryBuilder<DeleteExpressionPositions> queryBuilder, Func<object, string?>? datasetConverterer, ExpressionCompileOptions options)
        {
            using (_loggers.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                queryBuilder.ValidateArgument(nameof(queryBuilder));
                var builderExpressions = queryBuilder.Expressions;

                var expressionCount = builderExpressions.SelectMany(x => x.Value).GetCount();
                var isFormatted = options.HasFlag(ExpressionCompileOptions.Format);
                
                _loggers.Log($"{_name} compiling <{expressionCount}> expressions into a delete query");
                if (expressionCount == 0) throw new NotSupportedException($"No expressions to compile into a delete query");

                builder.Append(Sql.Statements.Delete);

                foreach (var builderExpression in builderExpressions.OrderBy(x => x.Key).Where(x => x.Value.HasValue()))
                {
                    var position = builderExpression.Key;
                    var expressions = builderExpression.Value;

                    // Position settings
                    var isConfigSet = _deletePositionConfigs.ContainsKey(position);
                    var isNewLine = isConfigSet ? _deletePositionConfigs[position].IsNewLine : false;
                    var clause = isConfigSet ? _deletePositionConfigs[position].Clause : null;
                    var joinValue = (isConfigSet ? _deletePositionConfigs[position].ExpressionJoinValue : null) ?? Constants.Strings.Space;
                    var isSingleExpression = isConfigSet ? _deletePositionConfigs[position].IsSingleExpression : false;

                    _loggers.Debug($"{_name} compiling <{expressions.Length}> expressions for position <{position}>");

                    // Formatting
                    if (isFormatted && isNewLine)
                    {
                        builder.AppendLine();
                    }
                    else
                    {
                        builder.AppendSpace();
                    }

                    // Special cases
                    if (position == DeleteExpressionPositions.From)
                    {
                        if(expressions.Any(x => x is IObjectExpression || x is TableExpression))
                        {
                            var aliases = expressions.Where(x => x is IObjectExpression || x is TableExpression)
                                                        .Select(x => ConvertDataSet(x is TableExpression tableExpression ? tableExpression.DataSet?.DataSet : x.CastTo<IObjectExpression>().DataSet, datasetConverterer))
                                                        .Where(x => x != null).ToArray();

                            if (aliases.HasValue())
                            {
                                aliases.Execute((i, a) =>
                                {
                                    builder.Append(a);
                                    if (i != aliases.Length - 1) builder.Append(',');
                                });

                                // Formatting
                                if (isFormatted && isNewLine)
                                {
                                    builder.AppendLine();
                                }
                                else
                                {
                                    builder.AppendSpace();
                                }
                            }
                            else
                            {
                                _loggers.Warning($"{_name}: No table aliases defined. Using default format");
                            }
                        }
                        else
                        {
                            _loggers.Warning($"{_name}: No expression is <{nameof(IObjectExpression)}>. Won't be able to determine table to delete from. Using default format");
                        }
                    }

                    //Clause
                    if (clause.HasValue())
                    {
                        builder.Append(clause).AppendSpace();
                    }

                    // Single value
                    if (isSingleExpression)
                    {
                        if (expressions.Length > 1) _loggers.Warning($"Expected only 1 expression for position <{position}> but got <{expressions.Length}>. Taking the last expression");
                        CompileExpressionTo(builder, datasetConverterer, options, expressions.Last());
                        continue;
                    }
                    else
                    {
                        expressions.Execute((i, e) =>
                        {
                            var expressionJoinValue = joinValue == Environment.NewLine && !isFormatted ? Constants.Strings.Space : joinValue;

                            CompileExpressionTo(builder, datasetConverterer, options, e);
                            if (i < expressions.Length - 1) builder.Append(expressionJoinValue);
                        });
                    }
                }
            }
        }
        #endregion

        /// <inheritdoc/>
        public string Compile(IExpression expression, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            using (_loggers.TraceMethod(this))
            {
                expression.ValidateArgument(nameof(expression));

                return Compile(new StringBuilder(), expression, options).ToString();
            }                 
        }
        /// <inheritdoc/>
        public StringBuilder Compile(StringBuilder builder, IExpression expression, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            using (_loggers.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                expression.ValidateArgument(nameof(expression));

                CompileExpressionTo(builder, x => x?.ToString(), options, expression);
                if (options.HasFlag(ExpressionCompileOptions.AppendSeparator)) builder.Append(';');

                return builder;
            }
        }

        private void CompileExpressionTo(StringBuilder builder, Func<object, string?>? datasetConverterer, ExpressionCompileOptions options, IExpression expression, bool isInsert = false)
        {
            builder.ValidateArgument(nameof(builder));
            expression.ValidateArgument(nameof(expression));

            _loggers.Trace($"{_name} compiling expression of type <{expression.GetTypeName()}>");

            if(expression is TableExpression tableExpression)
            {
                // Remove schema as MySql doesn't support it
                tableExpression.Schema = null;
                tableExpression.ToSql(builder, (b, e) => CompileExpressionTo(b, datasetConverterer, options, e, isInsert), options);
            }
            else if (expression is IColumnExpression columnExpression && !columnExpression.Object.Equals(Sql.All.ToString()))
            {
                // Add back ticks around column names 
                columnExpression.ToSql(builder, x => ConvertDataSet(x, datasetConverterer, isInsert), x => $"`{x}`", true, options);
            }
            else if (expression is IObjectExpression objectExpression && !objectExpression.Object.Equals(Sql.All.ToString()))
            {
                // Add back ticks around column names 
                objectExpression.ToSql(builder, x => ConvertDataSet(x, datasetConverterer, isInsert), x => $"`{x}`", options);
            }
            else if (expression is IDataSetExpression dataSetExpression)
            {
                dataSetExpression.ToSql(builder, x => ConvertDataSet(x, datasetConverterer, isInsert), options);
            }
            else if (expression is IExpressionContainer expressionContainer)
            {
                expressionContainer.ToSql(builder, (b, e) => CompileExpressionTo(b, datasetConverterer, options, e, isInsert), options);
            }
            else
            {
                expression.ToSql(builder, options);
            }
        }

        private string? ConvertDataSet(object dataSet, Func<object, string?>? datasetConverterer, bool isInsert = false)
        {
            if (dataSet == null) return null;
            // Disable datasets in insert statements
            if (isInsert) return null;

            return datasetConverterer != null ? datasetConverterer(dataSet) : dataSet?.ToString();
        }

    }
}
