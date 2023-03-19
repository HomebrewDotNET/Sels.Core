using Sels.Core.Command.Linux.Templates.Attributes;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System.Collections;
using Sels.Core.Attributes.Enumeration.Value;

namespace Sels.Core.Command.Linux.Attributes
{
    /// <summary>
    /// Joins list values using a single format. So the end result becomes "{Flag} {Value} {Value} {Value} {Value}".
    /// </summary>
    public class TextListArgument : TextArgument
    {
        // Constants
        /// <summary>
        /// The default value for <see cref="ElementJoinVlaue"/>.
        /// </summary>
        public const string DefaultJoinValue = Constants.Strings.Space;

        // Properties
        /// <summary>
        /// String value used to join together all the values from the property values if it's a list
        /// </summary>
        public string ElementJoinVlaue { get; }

        /// <summary>
        /// Joins list values using a single format. So the end result becomes "{Flag} {Value} {Value} {Value} {Value}".
        /// </summary>
        /// <param name="prefix">Optional prefix that will be placed along side the property value based on <paramref name="format"/></param>
        /// <param name="format">How the <paramref name="prefix"/> and the element values should be formatted. Use <see cref="TextArgument.PrefixFormat"/> for the <paramref name="prefix"/> and <see cref="TextArgument.ValueFormat"/> for the joined element values</param>
        /// <param name="elementJoinValue">String that will join together all the elements in the property collection</param>
        /// <param name="parsingOption">Optional parsing for the element value</param>
        /// <param name="allowEmpty">If the argument should be generated when the property collection is empty</param>
        /// <param name="order">Used to order argument. Lower means it will get placed in the argument list first. Negative gets placed last in the argument list.</param>
        /// <param name="required">Indicates if this property must be set. Throws InvalidOperation when Required is true but property value is null.</param>
        public TextListArgument(string? prefix = null, string? format = null, string elementJoinValue = DefaultJoinValue, TextParsingOptions parsingOption = TextParsingOptions.None, bool allowEmpty = false, int order = LinuxCommandConstants.DefaultLinuxArgumentOrder, bool required = false) : base(prefix, format, parsingOption, allowEmpty, order, required)
        {
            ElementJoinVlaue = elementJoinValue.ValidateArgument(nameof(elementJoinValue));
        }
        /// <inheritdoc />
        public override string? CreateArgument(object? value = null)
        {
            if(value != null && !value.GetType().IsString() && value.GetType().IsContainer())
            {
                var values = new List<string>();

                foreach (var item in (IEnumerable)value)
                {
                    values.Add(ParsingOption.GetStringValue().FormatString(item.GetArgumentValue()));
                }

                if (!values.HasValue() && !AllowEmpty)
                {
                    return null;
                }

                var joinedValues = values.JoinString(ElementJoinVlaue);

                if (Prefix.HasValue())
                {
                    return Format.FormatString(Prefix, joinedValues);
                }

                return joinedValues;
            }
            else
            {
                return base.CreateArgument(value);
            }
        }
    }
}
