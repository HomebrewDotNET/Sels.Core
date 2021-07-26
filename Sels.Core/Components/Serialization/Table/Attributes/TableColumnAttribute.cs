using Sels.Core.Components.Conversion;
using Sels.Core.Contracts.Conversion;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Serialization.Table.Attributes
{
    /// <summary>
    /// Provides serialization info for <see cref="TableSerializer"/>.
    /// </summary>
    public class TableColumnAttribute : Attribute
    {
        // Properties
        /// <summary>
        /// Which column to deserialize to this property or serialize in to the column.
        /// </summary>
        public int ColumnIndex { get; }
        /// <summary>
        /// Optional converter for converting between string and property value.
        /// </summary>
        public IGenericTypeConverter Converter { get; }

        public TableColumnAttribute(int columnIndex, Type converterType = null, params object[] converterConstructorArguments)
        {
            ColumnIndex = columnIndex.ValidateArgumentLargerOrEqual(nameof(columnIndex), 0);

            if (converterType.HasValue())
            {
                converterType.ValidateArgumentAssignableTo(nameof(converterType), typeof(IGenericTypeConverter));
                converterType.ValidateArgumentCanBeContructedWithArguments(nameof(converterType), converterConstructorArguments);

                Converter = converterType.Construct<IGenericTypeConverter>(converterConstructorArguments);
            }
        }
    }
}
