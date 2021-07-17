using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Sels.Core.Components.Display.ObjectLabel
{
    public static class ObjectLabelExtensions
    {
        // Constants
        private const LabelFormat DefaultFormat = LabelFormat.UpperCaseToWords;
        private const string UpperCaseSplitRegex = @"(?<!^)(?=[A-Z])";

        public static string GetLabel(this object source)
        {
            return source.GetLabel(DefaultFormat);
        }
        public static string GetLabel(this object source, LabelFormat format)
        {
            source.ValidateVariable(nameof(source));

            return source.GetType().GetLabel(format);
        }
        public static string GetLabel(this PropertyInfo property)
        {
            return property.GetLabel(DefaultFormat);
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
        public static string GetLabel(this Type type)
        {
            return type.GetLabel(DefaultFormat);
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
        public static string GetLabel(this Enum enumValue)
        {
            enumValue.ValidateVariable(nameof(enumValue));

            return enumValue.GetLabel(DefaultFormat);
        }

        public static string GetLabel(this Enum enumValue, LabelFormat format)
        {
            enumValue.ValidateVariable(nameof(enumValue));

            var labelAttribute = enumValue.GetAttributeOrDefault<ObjectLabel>();

            if (labelAttribute.HasValue())
            {
                return labelAttribute.Label;
            }
            else
            {
                return format.FormatLabel(enumValue.ToString());
            }
        }

        public static string GetPropertyLabel(this object source, string propertyName)
        {
            return source.GetPropertyLabel(propertyName, DefaultFormat);
        }
        public static string GetPropertyLabel(this object source, string propertyName, LabelFormat format)
        {
            source.ValidateVariable(nameof(source));
            propertyName.ValidateVariable(nameof(propertyName));

            var property = source.GetPropertyInfo(propertyName);

            return property.GetLabel(format);
        }

        public static string GetPropertyLabel(this Type source, string propertyName)
        {
            return source.GetPropertyLabel(propertyName, DefaultFormat);
        }
        public static string GetPropertyLabel(this Type source, string propertyName, LabelFormat format)
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
                    return Regex.Split(label, UpperCaseSplitRegex).JoinStringSpace();
                default:
                    return label;

            }
        }
    }
}
