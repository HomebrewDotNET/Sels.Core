using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Sels.Core.Excel.Extensions;
using Sels.Core.Extensions.General.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Excel
{
    public class ExcelCursor
    {
        // Fields
        private readonly Worksheet _workSheet;

        // Properties
        public int CurrentRow { get; private set; }
        public int CurrentColumn { get; private set; }

        public ExcelCursor(string worksheetName, SpreadsheetDocument spreadSheet) : this()
        {
            spreadSheet.ValidateVariable(nameof(spreadSheet));
            worksheetName.ValidateVariable(nameof(worksheetName));

            _workSheet = spreadSheet.GetOrCreateWorksheet(worksheetName);
        }

        public ExcelCursor(Worksheet workSheet) : this()
        {
            workSheet.ValidateVariable(nameof(workSheet));

            _workSheet = workSheet;
        }

        private ExcelCursor()
        {
            CurrentRow = 1;
            CurrentColumn = 1;
        }

        #region Movement
        public void MoveTo(int rowIndex, int columnIndex)
        {
            MoveToRow(rowIndex);
            MoveToColumn(columnIndex);
        }

        public void MoveToRow(int rowIndex)
        {
            if(rowIndex <= 0)
            {
                CurrentRow = 0;
            }
            else
            {
                CurrentRow = rowIndex;
            }
        }

        public void MoveToColumn(int columnIndex)
        {
            if (columnIndex <= 0)
            {
                CurrentColumn = 0;
            }
            else
            {
                CurrentColumn = columnIndex;
            }
        }

        public void MoveToNext()
        {
            MoveToColumn(CurrentColumn++);
        }

        public void MoveToPrevious()
        {
            MoveToColumn(CurrentColumn--);
        }

        public void MoveUp()
        {
            MoveToRow(CurrentRow--);
        }

        public void MoveDown()
        {
            MoveToRow(CurrentRow++);
        }

        public void MoveToNext(int rowIndex)
        {
            MoveToNext();
            MoveToRow(rowIndex);
        }

        public void MoveToPrevious(int rowIndex)
        {
            MoveToPrevious();
            MoveToRow(rowIndex);
        }

        public void MoveUp(int columnIndex)
        {
            MoveUp();
            MoveToColumn(columnIndex);
        }

        public void MoveDown(int columnIndex)
        {
            MoveDown();
            MoveToColumn(columnIndex);
        }

        #endregion

        #region Set

        #endregion
    }
}
