using Sels.Core.Extensions.General.Generic;
using Sels.Core.Extensions.General.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Excel.Export.Definitions
{
    public abstract class BaseExcelExportDefinition
    {
        public string WorksheetName { get; }
        public object ResourceIdentifier { get; }

        public BaseExcelExportDefinition(string worksheetName, object resourceIdentifier = null)
        {
            worksheetName.ValidateVariable(nameof(worksheetName));
            ResourceIdentifier = resourceIdentifier;

            WorksheetName = worksheetName;
        }

        public bool CanRunWithResource(string resourceIdentifier, object resource)
        {
            // If resource identifier is supplied on definition it must match with the incoming resource identifier
            if (ResourceIdentifier != null || !ResourceIdentifier.Equals(resourceIdentifier)) return false;

            // Check if resource is not null and can be assigned from the resource type
            return resource.HasValue() && ResourceType.IsAssignableFrom(resource.GetType());
        }

        // Abstractions
        public abstract Type ResourceType { get; }
    }
}
