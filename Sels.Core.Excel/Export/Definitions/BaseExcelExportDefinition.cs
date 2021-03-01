using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Excel.Export.Definitions
{
    public abstract class BaseExcelExportDefinition
    {
        // Fields

        // Properties
        public object ResourceIdentifier { get; }
        public SeekMode SeekMode { get; }

        public BaseExcelExportDefinition(SeekMode seekmode, object resourceIdentifier = null)
        {
            ResourceIdentifier = resourceIdentifier;
            SeekMode = seekmode;
        }

        public bool CanRunWithResource(object resourceIdentifier, object resource)
        {
            // If resource identifier is supplied on definition it must match with the incoming resource identifier
            if (ResourceIdentifier != null && !ResourceIdentifier.Equals(resourceIdentifier)) return false;

            // Check if resource is not null and can be assigned from the resource type
            var hasItems = resource.HasValue();
            var canBeAssigned = false;
            if(resource != null)
            {
                var resourceType = resource.GetType();
                canBeAssigned = ResourceType.IsAssignableFrom(resourceType);
            }
             
            return hasItems && canBeAssigned;
        }

        protected CellType GetCellTypeFromType(Type type)
        {
            type.ValidateVariable(nameof(type));

            if (type.IsNumeric())
            {
                return CellType.Numeric;
            }

            return CellType.String;
        }

        // Abstractions
        public abstract Type ResourceType { get; }

        public abstract void Export(ExcelCursor cursor, object resource);
    }
}
