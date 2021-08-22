using Sels.Core.Components.Enumeration.Value;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Object;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Linux.Extensions;
using Sels.Core.Linux.Extensions.Argument;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Attributes
{
    /// <summary>
    /// Creates argument with property value based on optional prefix and format. Uses ToString of object or the value defined in the <see cref="LinuxValueAttribute"/>. Each value in a list will be join together using the following format: "{Flag} {ItemValue}, {Flag} {ItemValue}, {Flag} {ItemValue}, ..."
    /// </summary>
    public class TextArgument : LinuxArgument
    {
        // Constants
        public const string PrefixFormat = "{Name}";
        public const string ValueFormat = "{Value}";
        public const string DefaultFormat = PrefixFormat + Constants.Strings.Space + ValueFormat;

        // Properties
        /// <summary>
        /// Optional prefix that gets added to the argument using the defined Format
        /// </summary>
        public string Prefix { get; }
        /// <summary>
        /// How to join the prefix and property value together if Prefix is defined. Prefix:<see cref="PrefixFormat"/> | Value: <see cref="ValueFormat"/>
        /// </summary>
        public string Format { get; }
        /// <summary>
        /// Optional parsing option for formatting any property values
        /// </summary>
        public TextParsingOptions ParsingOption { get; }
        /// <summary>
        /// Allow null values to be parsed. If they aren't allowed no argument value gets generated if property value is null
        /// </summary>
        public bool AllowEmpty { get; }

        /// <summary>
        /// Defines an argument for a linux command that will be sourced from the property <see cref="object.ToString"/> value. Collection elements will be joined together using a space.
        /// </summary>
        /// <param name="prefix">Optional prefix that will be placed along side the property value based on <paramref name="format"/></param>
        /// <param name="format">How the <paramref name="prefix"/> and property value should be formatted. Use <see cref="PrefixFormat"/> for the <paramref name="prefix"/> and <see cref="ValueFormat"/> for the property value</param>
        /// <param name="parsingOption">Optional parsing for the property value</param>
        /// <param name="allowEmpty">If an argument should be generated when the property value string is empty. If not allowed and the property value string is <see cref="string.Empty"/> null will be returned</param>
        /// <param name="order">Used to order argument. Lower means it will get placed in the argument list first. Negative gets placed last in the argument list.</param>
        /// <param name="required">Indicates if this property must be set. Throws InvalidOperation when Required is true but property value is null.</param>
        public TextArgument(string prefix = null, string format = null, TextParsingOptions parsingOption = TextParsingOptions.None, bool allowEmpty = false, int order = LinuxConstants.DefaultLinuxArgumentOrder, bool required = false) : base(true, order, required)
        {
            Prefix = prefix;

            Format = format.HasValue() ? format : DefaultFormat;

            ParsingOption = parsingOption;

            AllowEmpty = allowEmpty;
        }

        /// <summary>
        /// Defines an argument for a linux command that will be sourced from the property <see cref="object.ToString"/> value. Collection elements will be joined together using a space.
        /// </summary>
        /// <param name="prefix">Optional prefix that will be placed along side the property value based on <paramref name="format"/></param>
        /// <param name="format">How the <paramref name="prefix"/> and property value should be formatted. Use <see cref="PrefixFormat"/> for the <paramref name="prefix"/> and <see cref="ValueFormat"/> for the property value</param>
        /// <param name="parsingOption">Optional parsing for the property value</param>
        /// <param name="allowEmpty">If an argument should be generated when the property value string is empty. If not allowed and the property value string is <see cref="string.Empty"/> null will be returned</param>
        /// <param name="convertToPrimitive">If all non primitive types should be converted to string. If enabled all non primitive types and non primitive elements in collections will be converted to string using <see cref="object.ToString()"/></param>
        /// <param name="order">Used to order argument. Lower means it will get placed in the argument list first. Negative gets placed last in the argument list.</param>
        /// <param name="required">Indicates if this property must be set. Throws InvalidOperation when Required is true but property value is null.</param>
        protected TextArgument(string prefix = null, string format = null, TextParsingOptions parsingOption = TextParsingOptions.None, bool allowEmpty = false, bool convertToPrimitive = true, int order = LinuxConstants.DefaultLinuxArgumentOrder, bool required = false) : base(convertToPrimitive, order, required)
        {
            Prefix = prefix;

            Format = format.HasValue() ? format : DefaultFormat;

            ParsingOption = parsingOption;

            AllowEmpty = allowEmpty;
        }


        private string ParseValue(string value)
        {
            if (Prefix.HasValue())
            {
                return Format.Replace(PrefixFormat, Prefix).Replace(ValueFormat, ParsingOption.GetValue().FormatString(value));
            }

            return ParsingOption.GetValue().FormatString(value);
        }

        public override string CreateArgument(object value = null)
        {
            value ??= string.Empty;

            if(!AllowEmpty && value.Equals(string.Empty))
            {
                return null;
            }
            else if (!value.GetType().IsString() && value.GetType().IsItemContainer())
            {
                var values = new List<string>();

                foreach(var item in (IEnumerable)value)
                {
                    values.Add(ParseValue(item.GetArgumentValue()));
                }

                if (!values.HasValue() && !AllowEmpty)
                {
                    return null;
                }

                return values.JoinStringSpace();
            }

            return ParseValue(value.GetArgumentValue());
        }
    }

    public enum TextParsingOptions
    {
        [EnumValue("{0}")]
        None,
        [EnumValue("'{0}'")]
        Quotes,
        [EnumValue("\"{0}\"")]
        DoubleQuotes
    }
}
