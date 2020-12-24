using Sels.Core.Extensions.General.Generic;
using Sels.Core.Extensions.General.Validation;
using Sels.Core.Extensions.Reflection.Object;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Sels.Core.Extensions.Object.ItemContainer;
using Sels.Core.Extensions.Object.String;

namespace Sels.Core.Components.Display.PropertyLabel
{
    public static class PropertyLabelExtensions
    {
        private const string UpperCaseSplitRegex = @"(?<!^)(?=[A-Z])";

        public static string GetLabel(this PropertyInfo property)
        {
            return property.GetLabel(LabelFormat.UpperCaseToWords);
        }

        public static string GetLabel(this Type type)
        {
            return type.GetLabel(LabelFormat.UpperCaseToWords);
        }

        public static string GetLabel(this PropertyInfo property, LabelFormat format)
        {
            property.ValidateVariable(nameof(property));

            var labelAttribute = property.GetAttributeOrDefault<PropertyLabel>();

            if (labelAttribute.HasValue())
            {
                return labelAttribute.Label;
            }
            else
            {
                return format.FormatLabel(property.Name);
            }
        }

        public static string GetLabel(this Type type, LabelFormat format)
        {
            type.ValidateVariable(nameof(type));

            var labelAttribute = type.GetAttributeOrDefault<PropertyLabel>();

            if (labelAttribute.HasValue())
            {
                return labelAttribute.Label;
            }
            else
            {
                return format.FormatLabel(type.Name);
            }
        }

        private static string FormatLabel(this LabelFormat format, string label)
        {
            switch (format)
            {
                case LabelFormat.UpperCaseToWords:
                    return Regex.Split(label, UpperCaseSplitRegex).JoinSpace();
                default:
                    return label;

            }
        }
    }
}
