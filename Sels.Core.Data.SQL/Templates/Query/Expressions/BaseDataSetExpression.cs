﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Template for creating a <see cref="IDataSetExpression"/>.
    /// </summary>
    public abstract class BaseDataSetExpression : BaseExpression, IDataSetExpression
    {
        /// <inheritdoc/>
        public object? DataSet { get; }

        ///<inheritdoc cref="BaseDataSetExpression"/>
        /// <param name="dataset"><inheritdoc cref="DataSet"/></param>
        public BaseDataSetExpression(object? dataset)
        {
            DataSet = dataset;
        }
        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            ToSql(builder, x => x.ToString(), options);
        }

        /// <inheritdoc/>
        public abstract void ToSql(StringBuilder builder, Func<object, string?> datasetConverterer, ExpressionCompileOptions options = ExpressionCompileOptions.None);
    }
}
