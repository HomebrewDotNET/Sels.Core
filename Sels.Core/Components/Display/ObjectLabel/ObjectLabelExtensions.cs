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

namespace Sels.Core.Components.Display.ObjectLabel
{
    public static class ObjectLabelExtensions
    {
        private const string UpperCaseSplitRegex = @"(?<!^)(?=[A-Z])";
        public static string GetLabel(this object source)
        {
            return source.GetLabel(LabelFormat.UpperCaseToWords);
        }
        public static string GetLabel(this PropertyInfo property)
        {
            return property.GetLabel(LabelFormat.UpperCaseToWords);
        }

        public static string GetLabel(this Type type)
        {
            return type.GetLabel(LabelFormat.UpperCaseToWords);
        }

        public static string GetPropertyLabel(this object source, string propertyName)
        {
            return source.GetPropertyLabel(propertyName, LabelFormat.UpperCaseToWords);
        }

        public static string GetLabel(this object source, LabelFormat format)
        {
            source.ValidateVariable(nameof(source));

            return source.GetType().GetLabel(format);
        }

        public static string GetLabel(this PropertyInfo property, LabelFormat format)
        {
            property.ValidateVariable(nameof(property));

            var labelAttribute = property.GetAttributeOrDefault<ObjectLabel>();

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

            var labelAttribute = type.GetAttributeOrDefault<ObjectLabel>();

            if (labelAttribute.HasValue())
            {
                return labelAttribute.Label;
            }
            else
            {
                return format.FormatLabel(type.Name);
            }
        }

        public static string GetPropertyLabel(this object source, string propertyName, LabelFormat format)
        {
            source.ValidateVariable(nameof(source));
            propertyName.ValidateVariable(nameof(propertyName));

            var property = source.GetPropertyInfo(propertyName);

            return property.GetLabel(format);
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
