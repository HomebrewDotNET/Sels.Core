using DocumentFormat.OpenXml.Spreadsheet;
using Sels.Core.Extensions;
using Sels.Core.Extensions;
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

        public Func<TResource, CellFormula> FormulaGetter { get; }

        public ExcelTableColumnDefinition(string header, Func<TResource, object> valueGetter, CellType columnCellType)
        {
            valueGetter.ValidateVariable(nameof(valueGetter));

            Header = header ?? string.Empty;
            ColumnCellType = columnCellType;
            ValueGetter = valueGetter;
        }

        public ExcelTableColumnDefinition(string header, Func<TResource, object> valueGetter, CellType columnCellType, Func<TResource, CellFormula> formulaGetter) : this(header, valueGetter, columnCellType)
        {
            FormulaGetter = formulaGetter;
        }

        public object GetValue(TResource resource)
        {
            return ValueGetter(resource);
        }

        public CellFormula GetCellFormula(TResource resource)
        {
            if (FormulaGetter.HasValue())
            {
                return FormulaGetter(resource);
            }
            else
            {
                return default;
            }
        }
    }
}
