using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions;

namespace Sels.SQL.QueryBuilder.Expressions
{
    /// <summary>
    /// Expression that represents an SQL variable.
    /// </summary>
    public class VariableExpression : BaseExpression
    {
        // Constants
        /// <summary>
        /// The default variable prefix used.
        /// </summary>
        public const char DefaultPrefix = Sql.VariablePrefix;

        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string Name { get; }

        /// <inheritdoc cref="VariableExpression"/>
        /// <param name="name"><inheritdoc cref="Name"/></param>
        public VariableExpression(string name)
        {
            Name = name.ValidateArgumentNotNullOrWhitespace(nameof(name));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));

            if(!Name.StartsWith(DefaultPrefix)) builder.Append(DefaultPrefix);
            builder.Append(Name);
        }
    }
}
