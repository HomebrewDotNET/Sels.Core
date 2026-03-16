using Sels.Core.Conversion.Templates.Table;
using Sels.Core.Extensions;
using System;
using System.Linq;

namespace Sels.Core.Conversion.Attributes.Table
{
    /// <summary>
    /// Defines a header name that will be used to determine what column to use for serialization/deserialization.
    /// </summary>
    public class ColumnHeaderAttribute : BaseColumnAttribute
    {
        // Properties
        /// <summary>
        /// The name of the column in the header row.
        /// </summary>
        public string Header { get; }

        /// <inheritdoc cref="ColumnHeaderAttribute"/>
        /// <param name="header">The name of the header</param>
        public ColumnHeaderAttribute(string header)
        {
            Header = header.ValidateArgumentNotNullOrWhitespace(nameof(header));
        }

        /// <inheritdoc />
        public override int GetColumnIndex(string[] headerRow)
        {
            headerRow.ValidateArgument(nameof(headerRow));

            var foundHeaderValue = headerRow.FirstOrDefault(x => x.Equals(Header, StringComparison.OrdinalIgnoreCase));
            return foundHeaderValue.HasValue() ? Array.IndexOf(headerRow, foundHeaderValue) : -1;
        }
    }
}
