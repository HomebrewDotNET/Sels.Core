﻿using Sels.Core.Data.SQL.Query;
using Sels.Core.Data.SQL.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Compilation
{
    /// <summary>
    /// Compiler that converts <see cref="IExpression"/> into sql.
    /// </summary>
    public interface IExpressionCompiler
    {
        /// <summary>
        /// Compiles <paramref name="expression"/> into a sql string.
        /// </summary>
        /// <param name="expression">The expression to compile into sql</param>
        /// <param name="options"><inheritdoc cref="ExpressionCompileOptions"/></param>
        /// <returns><paramref name="expression"/> compiled into an sql string</returns>
        string Compile(IExpression expression, ExpressionCompileOptions options = ExpressionCompileOptions.None);
        /// <summary>
        /// Compiler <paramref name="expression"/> into a sql string and appends it to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder to add the sql string to</param>
        /// <param name="expression">The expression to compile into sql</param>
        /// <param name="options"><inheritdoc cref="ExpressionCompileOptions"/></param>
        /// <returns><paramref name="builder"/> for method chaining</returns>
        StringBuilder Compile(StringBuilder builder, IExpression expression, ExpressionCompileOptions options = ExpressionCompileOptions.None);
    }
}
