using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.Builder.Expressions
{
    /// <summary>
    /// Base class for creating expressions that require custom compiler support.
    /// </summary>
    public abstract class CustomCompilerExpression : IExpression
    {
        /// <inheritdoc/>
        public virtual void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None) => throw new NotSupportedException($"This expression requires custom compiler support. Implementation is different for each RDBMS");
    }
}
