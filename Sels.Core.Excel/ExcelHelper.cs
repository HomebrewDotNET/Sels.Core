using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Excel
{
    public static class ExcelHelper
    {
        public static class Styles
        {
            public const uint Bold = 1U;
        }

        // Constants
        public const string HyperlinkFormat = @"HYPERLINK(""{0}"", ""{1}"")";

        public static CellFormula GetHyperlinkCellFormula(string hyperlink, string name)
        {
            hyperlink.ValidateVariable(nameof(hyperlink));
            name.ValidateVariable(nameof(name));


            CellFormula cellFormula = new CellFormula() { Space = SpaceProcessingModeValues.Preserve };
            cellFormula.Text = HyperlinkFormat.FormatString(hyperlink, name);

            return cellFormula;
        }
    }
}
