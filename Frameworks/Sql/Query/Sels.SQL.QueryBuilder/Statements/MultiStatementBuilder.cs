using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions.Linq;

namespace Sels.SQL.QueryBuilder.Statements
{
    /// <summary>
    /// Builder for creating a MySql query consisting of multiple statements and/ore expressions.
    /// </summary>
    public class MultiStatementBuilder : IMultiStatementBuilder
    {
        // Fields
        private readonly IExpressionCompiler _compiler;
        private readonly List<(IExpression Expression, Action<StringBuilder, ExpressionCompileOptions> BuildAction, bool IsFullStatement)> _builderActions = new List<(IExpression Expression, Action<StringBuilder, ExpressionCompileOptions> BuildAction, bool IsFullStatement)>();

        // Properties
        /// <inheritdoc/>
        public IExpression[] InnerExpressions => _builderActions.Select(x => x.Expression).ToArray();

        /// <inheritdoc cref="MultiStatementBuilder"/>
        /// <param name="compiler">The compiler used to convert expressions and builder into Sql queries</param>
        public MultiStatementBuilder(IExpressionCompiler compiler)
        {
            _compiler = compiler.ValidateArgument(nameof(compiler));
        }

        /// <inheritdoc/>
        public IMultiStatementBuilder Append(IQueryBuilder builder)
        {
            builder.ValidateArgument(nameof(builder));

            var expression = new SubQueryExpression(null, builder, false);

            // Query builder should always be a full statement so default to true
            _builderActions.Add((expression, (b, o) => builder.Build(b, o), true));

            return this;
        }
        /// <inheritdoc/>
        public IMultiStatementBuilder Append(IExpression expression, bool isFullStatement = true)
        {
            expression.ValidateArgument(nameof(expression));

            _builderActions.Add((expression, (b, o) => {
                _compiler.Compile(b, expression, null, o);
                if (isFullStatement && o.HasFlag(ExpressionCompileOptions.AppendSeparator)) b.Append(';');
            }, isFullStatement));

            return this;
        }

        /// <inheritdoc/>
        public string Build(ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            return Build(new StringBuilder(), options).ToString();
        }
        /// <inheritdoc/>
        public StringBuilder Build(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));

            _builderActions.Execute((i, b) =>
            {
                var (expression, action, isFullStatement) = b;
                // Remove append option if the expression isn't a full statement
                action(builder, isFullStatement ? options : options & ~ExpressionCompileOptions.AppendSeparator);

                // Add extra line between statements
                if (i < _builderActions.Count - 1 && isFullStatement && options.HasFlag(ExpressionCompileOptions.Format)) builder.AppendLine();
            });

            return builder;
        }

        /// <inheritdoc/>
        public string TranslateToAlias(object alias) => alias?.ToString();
    }
}
