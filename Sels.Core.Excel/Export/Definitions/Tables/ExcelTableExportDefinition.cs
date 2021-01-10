using Sels.Core.Extensions.General.Validation;
using Sels.Core.Extensions.Reflection.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Excel.Export.Definitions.Tables
{
    public class ExcelTableExportDefinition<TResource> : BaseExcelExportDefinition
    {
        // Fields
        private readonly List<ExcelTableColumnDefinition<TResource>> _columnDefinitions = new List<ExcelTableColumnDefinition<TResource>>();

        // Properties
        public bool GenerateHeaders { get; }
        public int StartRow { get; set; }

        public override Type ResourceType => typeof(TResource);

        public ExcelTableExportDefinition(string worksheetName, bool generateHeaders = true, int startRow = 1, object resourceIdentifier = null) : base(worksheetName, resourceIdentifier)
        {
            startRow.ValidateVariable(x => x >= 1, () => $"{nameof(startRow)} must be larger or equal to 1");

            GenerateHeaders = generateHeaders;
            StartRow = startRow;
        }

        #region Setup
        public ExcelTableExportDefinition<TResource> AddColumn<TResult>(Func<TResource, TResult> valueGetter)
        {
            valueGetter.ValidateVariable(nameof(valueGetter));

            CellType columnCellType = typeof(TResult).IsNumeric() ? CellType.Numeric : CellType.String;

            return AddColumn(string.Empty, x => valueGetter(x), columnCellType);
        }

        public ExcelTableExportDefinition<TResource> AddColumn<TResult>(string header, Func<TResource, TResult> valueGetter)
        {
            valueGetter.ValidateVariable(nameof(valueGetter));

            CellType columnCellType = typeof(TResult).IsNumeric() ? CellType.Numeric : CellType.String;

            return AddColumn(header, x => valueGetter(x), columnCellType);
        }

        public ExcelTableExportDefinition<TResource> AddColumn(Func<TResource, object> valueGetter, CellType columnCellType)
        {
            valueGetter.ValidateVariable(nameof(valueGetter));

            return AddColumn(string.Empty, valueGetter, columnCellType);
        }

        public ExcelTableExportDefinition<TResource> AddColumn(string header, Func<TResource, object> valueGetter, CellType columnCellType)
        {
            valueGetter.ValidateVariable(nameof(valueGetter));

            _columnDefinitions.Add(new ExcelTableColumnDefinition<TResource>(header, valueGetter, columnCellType));

            return this;
        }
        #endregion
    }
}
