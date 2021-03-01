using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Sels.Core.Excel.Extensions;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Excel
{
    public class ExcelCursor
    {
        // Fields
        private readonly Worksheet _worksheet;

        // Properties
        public uint CurrentRow { get; private set; }
        public uint CurrentColumn { get; private set; }

        public ExcelCursor(string worksheetName, SpreadsheetDocument spreadSheet) : this()
        {
            spreadSheet.ValidateVariable(nameof(spreadSheet));
            worksheetName.ValidateVariable(nameof(worksheetName));

            _worksheet = spreadSheet.GetOrCreateWorksheet(worksheetName);
        }

        public ExcelCursor(Worksheet worksheet) : this()
        {
            worksheet.ValidateVariable(nameof(worksheet));

            _worksheet = worksheet;
        }

        private ExcelCursor()
        {
            CurrentRow = 1;
            CurrentColumn = 1;
        }

        #region Movement
        public void MoveTo(uint rowIndex, uint columnIndex)
        {
            MoveToRow(rowIndex);
            MoveToColumn(columnIndex);
        }

        public void MoveToRow(uint rowIndex)
        {
            if(rowIndex <= 1)
            {
                CurrentRow = 1;
            }
            else
            {
                CurrentRow = rowIndex;
            }
        }

        public void MoveToColumn(uint columnIndex)
        {
            if (columnIndex <= 1)
            {
                CurrentColumn = 1;
            }
            else
            {
                CurrentColumn = columnIndex;
            }
        }

        public void MoveToNext()
        {
            MoveToColumn(CurrentColumn+1);
        }

        public void MoveToPrevious()
        {
            MoveToColumn(CurrentColumn-1);
        }

        public void MoveUp()
        {
            MoveToRow(CurrentRow-1);
        }

        public void MoveDown()
        {
            MoveToRow(CurrentRow+1);
        }

        public void MoveToNext(uint rowIndex)
        {
            MoveToNext();
            MoveToRow(rowIndex);
        }

        public void MoveToPrevious(uint rowIndex)
        {
            MoveToPrevious();
            MoveToRow(rowIndex);
        }

        public void MoveUp(uint columnIndex)
        {
            MoveUp();
            MoveToColumn(columnIndex);
        }

        public void MoveDown(uint columnIndex)
        {
            MoveDown();
            MoveToColumn(columnIndex);
        }

        public void SeekNextFreeColumn(uint offset = 1)
        {
            var nextFreeColumn = _worksheet.SeekFirstColumnWithNoContentAfter();

            MoveTo(1, nextFreeColumn == 1 ? 1 : nextFreeColumn + offset);
        }

        public void SeekNextFreeRow(uint offset = 1)
        {
            var nextFreeRow = _worksheet.SeekFirstRowWithNoContentAfter();

            MoveTo(nextFreeRow == 1 ? 1 : nextFreeRow + offset, 1);
        }

        public void SeekNextFreeColumnAfterCurrentRow(uint offset = 1)
        {
            var nextFreeColumn = _worksheet.SeekFirstColumnWithNoContentAfter(CurrentRow);

            MoveToColumn(nextFreeColumn == 1 ? 1 : nextFreeColumn + offset);
        }

        public void SeekNextFreeRowAfterCurrentColumn(uint offset = 1)
        {
            var nextFreeRow = _worksheet.SeekFirstRowWithNoContentAfter(CurrentColumn);

            MoveToRow(nextFreeRow == 1 ? 1 : nextFreeRow + offset);
        }

        #endregion

        #region Set
        public void SetValue(string value, CellType type, CellFormula cellFormula = null)
        {
            var row = _worksheet.CreateRowIfNotExistsAtIndex(CurrentRow);
            var cell = row.GetCellAtIndex(CurrentColumn);            

            if (!cell.HasValue())
            {
                cell = new Cell() {
                    CellReference = CurrentColumn.ToCellReference(row.RowIndex.ToUInt32())
                };

                row.AddCell(cell);
            }

            cell.CellValue = new CellValue(value);
            cell.DataType = type.ToCellValueType();

            if (cellFormula != null)
            {
                cell.CellFormula = cellFormula;
            }
        }
        #endregion
    }
}
