using Microsoft.Extensions.Logging;
using Sels.Core.Data.SQL.Query;
using Sels.Core.Data.SQL.Query.Compilation;
using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.MySQL.Query.Compiling
{
    /// <summary>
    /// Query compiler that converts sql expressions to the mysql syntax.
    /// </summary>
    public class MySqlCompiler :
        IQueryCompiler<InsertExpressionPositions>,
        IQueryCompiler<SelectExpressionPositions>,
        IQueryCompiler<DeleteExpressionPositions>
    {
        // Fields
        private readonly IEnumerable<ILogger>? _loggers;
        private readonly string _name = nameof(MySqlCompiler);

        #region Configs
        private static readonly IReadOnlyDictionary<InsertExpressionPositions, (bool IsNewLine, string? Prefix, string[]? ExpressionJoinValues, string? Suffix, bool IsSingleExpression)> _insertPositionConfigs = new Dictionary<InsertExpressionPositions, (bool IsNewLine, string? Prefix, string[]? ExpressionJoinValues, string? Suffix, bool IsSingleExpression)>()
        {
            { InsertExpressionPositions.Into, (false, null, null, null, true) },
            { InsertExpressionPositions.Columns, (false, "(", new string[] {  Constants.Strings.Comma}, ")", false) },
            { InsertExpressionPositions.Values, (true, Sql.Clauses.Values + Constants.Strings.Space, new string[]{ Constants.Strings.Comma , Environment.NewLine}, null, false) }
        };
        private static readonly IReadOnlyDictionary<SelectExpressionPositions, (bool IsNewLine, string? Clause, string? ExpressionJoinValue, bool IsSingleExpression)> _selectPositionConfigs = new Dictionary<SelectExpressionPositions, (bool IsNewLine, string? Clause, string? ExpressionJoinValue, bool IsSingleExpression)>()
        {
            { SelectExpressionPositions.Column, (false, null, Constants.Strings.Comma, false) },
            { SelectExpressionPositions.From, (true, Sql.Clauses.From, null, true) },
            { SelectExpressionPositions.Join, (true, null, Environment.NewLine, false) },
            { SelectExpressionPositions.Where, (true, Sql.Clauses.Where, Constants.Strings.Space, false) },
            { SelectExpressionPositions.OrderBy, (true, Sql.Clauses.OrderBy, Constants.Strings.Comma, false) },
            { SelectExpressionPositions.GroupBy, (true, Sql.Clauses.GroupBy, Constants.Strings.Comma, false) },
            { SelectExpressionPositions.After, (true, null, Environment.NewLine, false) },
        };
        private static readonly IReadOnlyDictionary<DeleteExpressionPositions, (bool IsNewLine, string? Clause, string? ExpressionJoinValue, bool IsSingleExpression)> _deletePositionConfigs = new Dictionary<DeleteExpressionPositions, (bool IsNewLine, string? Clause, string? ExpressionJoinValue, bool IsSingleExpression)>()
        {
            { DeleteExpressionPositions.From, (true, Sql.Clauses.From, Constants.Strings.Comma, false) },
            { DeleteExpressionPositions.Join, (true, null, Environment.NewLine, false) },
            { DeleteExpressionPositions.Where, (true, Sql.Clauses.Where, Constants.Strings.Space, false) },
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
        void IQueryCompiler<InsertExpressionPositions>.CompileTo(StringBuilder builder, IQueryBuilder queryBuilder, IReadOnlyDictionary<InsertExpressionPositions, IExpression[]> builderExpressions, QueryBuilderOptions options)
        {
            using (_loggers.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                queryBuilder.ValidateArgument(nameof(queryBuilder));
                builderExpressions.ValidateArgument(nameof(builderExpressions));

                var expressionCount = builderExpressions.SelectMany(x => x.Value).GetCount();
                var isFormatted = options.HasFlag(QueryBuilderOptions.Format);

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
                        builder.Append(prefix);
                    }

                    // Single value
                    if (isSingleExpression)
                    {
                        if (expressions.Length > 1) _loggers.Warning($"Expected only 1 expression for position <{position}> but got <{expressions.Length}>. Taking the last expression");
                        CompileExpressionTo(builder, queryBuilder, options, expressions.Last());
                    }
                    else
                    {
                        expressions.Execute((i, e) =>
                        {

                            CompileExpressionTo(builder, queryBuilder, options, e);
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
        void IQueryCompiler<SelectExpressionPositions>.CompileTo(StringBuilder builder, IQueryBuilder queryBuilder, IReadOnlyDictionary<SelectExpressionPositions, IExpression[]> builderExpressions, QueryBuilderOptions options)
        {
            using (_loggers.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                queryBuilder.ValidateArgument(nameof(queryBuilder));
                builderExpressions.ValidateArgument(nameof(builderExpressions));

                var expressionCount = builderExpressions.SelectMany(x => x.Value).GetCount();
                var isFormatted = options.HasFlag(QueryBuilderOptions.Format);

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
                        CompileExpressionTo(builder, queryBuilder, options, expressions.Last());
                        continue;
                    }
                    else
                    {
                        expressions.Execute((i, e) =>
                        {
                            var expressionJoinValue = joinValue == Environment.NewLine && !isFormatted ? Constants.Strings.Space : joinValue;

                            CompileExpressionTo(builder, queryBuilder, options, e);
                            if (i < expressions.Length - 1) builder.Append(expressionJoinValue);
                        });
                    }
                }
            }
        }
        #endregion

        #region Delete
        /// <inheritdoc/>
        void IQueryCompiler<DeleteExpressionPositions>.CompileTo(StringBuilder builder, IQueryBuilder queryBuilder, IReadOnlyDictionary<DeleteExpressionPositions, IExpression[]> builderExpressions, QueryBuilderOptions options)
        {
            using (_loggers.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                queryBuilder.ValidateArgument(nameof(queryBuilder));
                builderExpressions.ValidateArgument(nameof(builderExpressions));

                var expressionCount = builderExpressions.SelectMany(x => x.Value).GetCount();
                var isFormatted = options.HasFlag(QueryBuilderOptions.Format);
                
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
                        if(expressions.Any(x => x is IObjectExpression))
                        {
                            var aliases = expressions.Where(x => x is IObjectExpression).Cast<IObjectExpression>().Select(x => ConvertDataSet(x.DataSet, queryBuilder)).Where(x => x != null).ToArray();

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
                        CompileExpressionTo(builder, queryBuilder, options, expressions.Last());
                        continue;
                    }
                    else
                    {
                        expressions.Execute((i, e) =>
                        {
                            var expressionJoinValue = joinValue == Environment.NewLine && !isFormatted ? Constants.Strings.Space : joinValue;

                            CompileExpressionTo(builder, queryBuilder, options, e);
                            if (i < expressions.Length - 1) builder.Append(expressionJoinValue);
                        });
                    }
                }
            }
        }
        #endregion

        private void CompileExpressionTo(StringBuilder builder, IQueryBuilder queryBuilder, QueryBuilderOptions options, IExpression expression)
        {
            builder.ValidateArgument(nameof(builder));
            queryBuilder.ValidateArgument(nameof(queryBuilder));
            expression.ValidateArgument(nameof(expression));

            _loggers.Trace($"{_name} compiling expression of type <{expression.GetTypeName()}>");

            if (expression is IDataSetExpression dataSetExpression)
            {
                dataSetExpression.ToSql(builder, x => ConvertDataSet(x, queryBuilder), options);
            }
            else if (expression is IExpressionContainer expressionContainer)
            {
                expressionContainer.ToSql(builder, (b, e) => CompileExpressionTo(b, queryBuilder, options, e), options);
            }
            else
            {
                expression.ToSql(builder, options);
            }
        }

        private string? ConvertDataSet(object dataSet, IQueryBuilder queryBuilder)
        {
            if (dataSet == null) return null;
            return dataSet is Type type ? queryBuilder.GetAlias(type) : dataSet.ToString();
        }
    }
}
