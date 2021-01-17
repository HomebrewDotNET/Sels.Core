using Sels.Core.Extensions.General.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Excel.Export.Definitions.Tables
{
    internal class ExcelTableColumnDefinition<TResource>
    {
        // Properties
        public string Header { get; }
        public Func<TResource, object> ValueGetter { get; }
        public CellType ColumnCellType { get; }

        public ExcelTableColumnDefinition(string header, Func<TResource, object> valueGetter, CellType columnCellType)
        {
            valueGetter.ValidateVariable(nameof(valueGetter));

            Header = header ?? string.Empty;
            ColumnCellType = columnCellType;
            ValueGetter = valueGetter;
        }

        public object GetValue(TResource resource)
        {
            return ValueGetter(resource);
        }
    }
}
