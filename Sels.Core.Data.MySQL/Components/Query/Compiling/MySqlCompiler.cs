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
    public class MySqlCompiler : IQueryCompiler<SelectExpressionPositions>
    {
        // Fields
        private readonly IEnumerable<ILogger>? _loggers;
        private readonly string _name = nameof(MySqlCompiler);

        #region Select
        private static readonly IReadOnlyDictionary<SelectExpressionPositions, (bool IsNewLine, string? Clause, string? ExpressionJoinValue, bool IsSingleExpression)> _selectPositionConfigs = new Dictionary<SelectExpressionPositions, (bool IsNewLine, string? Clause, string? ExpressionJoinValue, bool IsSingleExpression)>()
        {
            { SelectExpressionPositions.Column, (false, null, Constants.Strings.Comma, false) },
            { SelectExpressionPositions.From, (true, Sql.Clauses.From, null, true) },
            { SelectExpressionPositions.Join, (true, null, Environment.NewLine, false) },
            { SelectExpressionPositions.Where, (true, Sql.Clauses.Where, Constants.Strings.Space, false) },
            { SelectExpressionPositions.OrderBy, (true, Sql.Clauses.OrderBy, Constants.Strings.Comma, false) },
            { SelectExpressionPositions.GroupBy, (true, Sql.Clauses.GroupBy, Constants.Strings.Comma, false) },
            { SelectExpressionPositions.End, (true, null, Environment.NewLine, false) },
        };
        #endregion

        /// <inheritdoc cref="MySqlCompiler"/>
        /// <param name="loggers"></param>
        public MySqlCompiler(IEnumerable<ILogger>? loggers = null)
        {
            _loggers = loggers;
        }

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

                _loggers.Log($"{_name} compiling <{expressionCount}> into a select query");
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
            return dataSet is Type type ? queryBuilder.GetAlias(type) : dataSet.ToString();
        }
    }
}
