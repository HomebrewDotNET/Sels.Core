using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Excel
{
    public class BaseExcelDefinition
    {
        // Properties
        public object ResourceIdentifier { get; }
        public Action<ExcelCursor> SetStartPositionAction { get; set; }

        public BaseExcelDefinition(Action<ExcelCursor> setStartPositionAction, object resourceIdentifier = null)
        {
            setStartPositionAction.ValidateVariable(nameof(setStartPositionAction));

            SetStartPositionAction = setStartPositionAction;
            ResourceIdentifier = resourceIdentifier;
        }

        public void SetStartPosition(ExcelCursor excelCursor) {
            SetStartPositionAction(excelCursor);
        }
    }
}
