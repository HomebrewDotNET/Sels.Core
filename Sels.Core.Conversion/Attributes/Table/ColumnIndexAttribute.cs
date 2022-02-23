using Sels.Core.Conversion.Templates.Table;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Conversion.Attributes.Table
{
    /// <summary>
    /// Defines the index of the column to serialize from or to deserialize to. Column index starts counting from 0.
    /// </summary>
    public class ColumnIndexAttribute : BaseColumnAttribute
    {
        // Properties
        /// <summary>
        /// The column index.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc cref="ColumnIndexAttribute"/>
        /// <param name="index"></param>
        public ColumnIndexAttribute(int index)
        {
            Index = index.ValidateArgumentLargerOrEqual(nameof(index), 0);
        }

        /// <inheritdoc/>
        public override int GetColumnIndex(string[] headerRow)
        {
            headerRow.ValidateArgument(nameof(headerRow));

            return Index <= headerRow.Length ? Index : -1;
        }
    }
}
