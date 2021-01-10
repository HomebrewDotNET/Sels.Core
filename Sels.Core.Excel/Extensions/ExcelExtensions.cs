using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Sels.Core.Extensions.General.Generic;
using Sels.Core.Extensions.General.Validation;
using Sels.Core.Extensions.Object.Number;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Excel.Extensions
{
    public static class ExcelExtensions
    {
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

            var workbookPart = spreadsheet.WorkbookPart;

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

            // Search for sheet by name
            var sheet = spreadsheet.WorkbookPart.Workbook.GetSheets().FirstOrDefault(x => x.Name.Equals(sheetName));

            if (sheet.HasValue())
            {
                var relationId = sheet.Id.Value;

                // Search for worksheet by relationId
                var worksheetPart = (WorksheetPart)spreadsheet.WorkbookPart.GetPartById(relationId);

                return worksheetPart.Worksheet;
            }

            return null;
        }

        public static List<Row> GetRows(this Worksheet worksheet)
        {
            return worksheet.GetFirstChild<SheetData>().Elements<Row>().ToList();
        }

        public static List<Cell> GetCells(this Row row)
        {
            return row.Elements<Cell>().ToList();
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

            return cells.FirstOrDefault(x => x.CellReference.ToString().ToCellIndex().Equals(cellIndex));
        }

        public static IEnumerable<Sheet> GetSheets(this Workbook workbook)
        {
            workbook.ValidateVariable(nameof(workbook));

            return workbook.Sheets.Cast<Sheet>();
        }
        #endregion

        #region Create

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
    }
}
