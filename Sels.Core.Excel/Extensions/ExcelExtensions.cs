using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Excel.Extensions
{
    public static class ExcelExtensions
    {
        // Constants 
        private const string FirstColumnReference = "A";
        private const int FirstColumn = 1;

        public static Worksheet GetOrCreateWorksheet(this SpreadsheetDocument spreadsheet, string sheetName)
        {
            spreadsheet.ValidateVariable(nameof(spreadsheet));
            sheetName.ValidateVariable(nameof(sheetName));

            var worksheet = spreadsheet.GetWorksheet(sheetName);

            if (!worksheet.HasValue())
            {
                worksheet = spreadsheet.CreateWorksheet(sheetName);
            }

            return worksheet;
        }

        public static Worksheet CreateWorksheet(this SpreadsheetDocument spreadsheet, string sheetName)
        {
            spreadsheet.ValidateVariable(nameof(spreadsheet));
            sheetName.ValidateVariable(nameof(sheetName));

            var workbookPart = spreadsheet.WorkbookPart ?? spreadsheet.AddWorkbookPart();
            workbookPart.Workbook ??= new Workbook();

            // Create new worksheet to contain data for sheet
            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            // Create new sheet and add relation to worksheet
            var newSheet = new Sheet()
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = workbookPart.Workbook.GenerateIdForSheet(),
                Name = sheetName
            };

            // Check if sheets already exist
            if (!workbookPart.Workbook.Sheets.HasValue())
            {
                // Create missing sheets element
                workbookPart.Workbook.AppendChild(new Sheets());
            }

            // Add sheet to workbook
            workbookPart.Workbook.Sheets.Append(newSheet);

            return worksheetPart.Worksheet;
        }

        #region Get
        public static Worksheet GetWorksheet(this SpreadsheetDocument spreadsheet, string sheetName)
        {
            spreadsheet.ValidateVariable(nameof(spreadsheet));
            sheetName.ValidateVariable(nameof(sheetName));

            if(spreadsheet?.WorkbookPart?.Workbook != null)
            {
                // Search for sheet by name
                var sheet = spreadsheet.WorkbookPart.Workbook.GetSheets().FirstOrDefault(x => x.Name.Equals(sheetName));

                if (sheet != null)
                {
                    var relationId = sheet.Id.Value;

                    // Search for worksheet by relationId
                    var worksheetPart = (WorksheetPart)spreadsheet.WorkbookPart.GetPartById(relationId);

                    return worksheetPart.Worksheet;
                }
            }

            return null;
        }

        public static List<Row> GetRows(this Worksheet worksheet, uint startRow = 1)
        {
            return worksheet.GetFirstChild<SheetData>().Elements<Row>().Where(x => x.RowIndex >= startRow).OrderBy(x => x.RowIndex).ToList();
        }

        public static List<Cell> GetCells(this Row row)
        {
            return row.Elements<Cell>().OrderBy(x => x.CellReference).ToList();
        }

        public static List<Cell> GetCells(this Worksheet worksheet, uint startRow = 1)
        {
            return worksheet.GetRows(startRow).SelectMany(x => x.GetCells()).ToList();
        }

        public static List<Cell> GetCellsAtColumn(this Worksheet worksheet, string cellReference, uint startRow = 1)
        {
            var parsedCellReference = cellReference.GetWithoutDigits();

            return worksheet.GetCells(startRow).Where(x => x.GetCellReferenceWithoutRow() == parsedCellReference).ToList();
        }

        public static Row GetRowAtIndex(this Worksheet worksheet, uint index)
        {
            var rows = worksheet.GetRows();

            return rows.FirstOrDefault(x => x.RowIndex.Equals(index));
        }

        public static Cell GetCellAtIndex(this Worksheet worksheet, uint rowIndex, uint cellIndex)
        {
            var row = worksheet.GetRowAtIndex(rowIndex);

            if (row.HasValue())
            {
                return row.GetCellAtIndex(cellIndex);
            }

            return null;
        }

        public static Cell GetCellAtIndex(this Row row, uint cellIndex)
        {
            var cells = row.GetCells();

            var cellReference = cellIndex.ToCellReference(row.RowIndex);

            return cells.FirstOrDefault(x => x.CellReference == cellReference);
        }

        public static IEnumerable<Sheet> GetSheets(this Workbook workbook)
        {
            workbook.ValidateVariable(nameof(workbook));

            if (workbook.Sheets.HasValue())
            {
                return workbook.Sheets.Cast<Sheet>();
            }

            return new List<Sheet>();
        }
        #endregion

        #region Exists
        public static bool ContainsRowAt(this Worksheet worksheet, uint index)
        {
            var rows = worksheet.GetRows();

            return rows.Any(x => x.RowIndex == index);
        }

        public static bool ContainsCellAt(this Row row, uint index)
        {
            var cellReference = index.ToCellReference(row.RowIndex.ToUInt32());

            return row.GetCells().Any(x => x.CellReference == cellReference);
        }

        public static bool ContainsCellAt(this Worksheet worksheet, uint rowIndex, uint cellIndex)
        {
            var row = worksheet.GetRowAtIndex(rowIndex);

            if(row != null)
            {
                return row.ContainsCellAt(cellIndex);
            }

            return false;
        }
        #endregion

        #region Create
        public static Row CreateRowIfNotExistsAtIndex(this Worksheet worksheet, uint index)
        {
            var row = GetRowAtIndex(worksheet, index);

            if (!row.HasValue())
            {
                row = new Row() { RowIndex = index };
                worksheet.SetRow(row);
            }

            return row;
        }

        public static void SetRow(this Worksheet worksheet, Row row)
        {
            var data = worksheet.GetFirstChild<SheetData>();

            //var index = ((uint)row.RowIndex).ToInt();

            data.Append(row);
        }

        public static void AddCell(this Row row, Cell cell)
        {
            //var cellIndex = cell.CellReference.ToString().ToCellIndex();

            row.Append(cell);
        }
        #endregion

        #region Search and Seeking
        public static uint SeekFirstRowWithNoContentAfter(this Worksheet worksheet, uint startColumn = 1)
        {
            uint lastFreeRow = 0;

            var rows = worksheet.GetRows();

            if (rows.HasValue())
            {
                foreach (var row in rows)
                {
                    var cells = row.GetCells().Where(x => x.CellReference.ToString().ToCellIndex() > startColumn);

                    // See if any cells contain values
                    var isEmpty = !cells.Any(x => x.CellValue.ToString().HasValue());

                    if (isEmpty)
                    {
                        // Set last free row if one isn't set yet
                        if (lastFreeRow == 0)
                        {
                            lastFreeRow = row.RowIndex;
                        }
                    }
                    else
                    {
                        // Row was not empty. Resetting last free row
                        lastFreeRow = 0;
                    }
                }
            }


            // Check if we have a last free row set. If last row was filled then the next should be free
            return lastFreeRow == 0 ? rows.Count().ToUInt32() + 1 : lastFreeRow;
        }

        public static uint SeekFirstColumnWithNoContentAfter(this Worksheet worksheet, uint startRow = 1)
        {
            uint lastFreeColumn = 0;

            // Get last defined column so we know when to stop looping
            var lastDefinedColumnIndex = worksheet.GetLastDefinedColumnIndex();

            // Go though each column and check all cell values at that column
            for(uint i = 1; i < lastDefinedColumnIndex; i++)
            {
                var cellReference = i.ToCellReference();

                var isEmpty = !worksheet.GetCellsAtColumn(cellReference, startRow).Any(x => x.CellValue.ToString().HasValue());

                if (isEmpty)
                {
                    // Set last free column if one isn't set yet
                    if (lastFreeColumn == 0)
                    {
                        lastFreeColumn = i;
                    }
                }
                else
                {
                    // Column was not empty. Resetting last free column
                    lastFreeColumn = 0;
                }
            }

            // Check if we have a last free column set. If last column was filled then the next should be free
            return lastFreeColumn == 0 ? lastDefinedColumnIndex + 1 : lastFreeColumn;
        }

        public static uint GetLastDefinedColumnIndex(this Worksheet worksheet)
        {
            return worksheet.GetCells().Select(x => x.CellReference.ToString().ToCellIndex()).OrderBy(x => x).LastOrDefault();
        }
        #endregion

        #region Delete
        public static void ClearCells(this Row row)
        {
            row.RemoveAllChildren<Cell>();
        }

        public static void ClearRows(this Worksheet worksheet)
        {
            worksheet.GetRows().Execute(x => x.ClearCells());

            worksheet.GetFirstChild<SheetData>().RemoveAllChildren<Row>();
        }
        #endregion

        public static uint GenerateIdForSheet(this Workbook workbook)
        {
            workbook.ValidateVariable(nameof(workbook));

            var sheets = workbook.GetSheets();

            var id = (sheets.Count()+1).ToUInt32();

            while(sheets.Any(x => x.SheetId.Equals(id)))
            {
                id++;
            }

            return id;
        }

        public static uint ToCellIndex(this string cellReference)
        {
            return cellReference.ToStringFromAlphaNumeric(26, 1);
        }

        public static string ToCellReference(this uint cellIndex)
        {
            return cellIndex.ToAlphaNumericString(26, 1);
        }

        public static string GetCellReferenceWithoutRow(this Cell cell)
        {
            return cell.CellReference.ToString().GetWithoutDigits();
        }

        public static string ToCellReference(this uint cellIndex, uint rowIndex)
        {
            return $"{cellIndex.ToAlphaNumericString(26, 1)}{rowIndex}";
        }

        public static EnumValue<CellValues> ToCellValueType(this CellType type)
        {
            switch (type)
            {
                case CellType.Numeric:
                    return CellValues.Number;
                default:
                    return CellValues.String;                  
            }
        }

        public static int ToInt(this UInt32Value intValue)
        {
            return intValue.ToUInt32().ToInt();
        }

        public static uint ToUInt32(this UInt32Value intValue)
        {
            return (uint)intValue;
        }
    }
}
