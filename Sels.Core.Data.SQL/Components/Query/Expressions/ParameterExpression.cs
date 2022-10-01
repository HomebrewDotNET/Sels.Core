using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that represents an sql parameter.
    /// </summary>
    public class ParameterExpression : BaseExpression, IExpression
    {
        // Constants
        /// <summary>
        /// The default parameter prefix used.
        /// </summary>
        public const char DefaultPrefix = Sql.ParameterPrefix;

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; }

        ///<inheritdoc cref="ParameterExpression"/>
        /// <param name="name"><see cref="Name"/></param>
        public ParameterExpression(string name)
        {
            Name = name.ValidateArgumentNotNullOrWhitespace(nameof(name));
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));

            ToSql(builder, DefaultPrefix.ToString(), options);
        }
        /// <summary>
        /// Converts the current expression to sql.
        /// </summary>
        /// <param name="builder">The builder to append to</param>
        /// <param name="parameterPrefix">The prefix to add before the parameter name</param>
        /// <param name="options">Optional settings for building the query</param>
        public void ToSql(StringBuilder builder, string parameterPrefix, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));

            if(!Name.StartsWith(parameterPrefix)) builder.Append(parameterPrefix);
            builder.Append(Name);
        }
    }
}
