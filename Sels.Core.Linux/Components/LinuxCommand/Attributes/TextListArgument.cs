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
    /// Joins list values using a single format. So the end result becomes "{Flag} {Value} {Value} {Value} {Value}".
    /// </summary>
    public class TextListArgument : TextArgument
    {
        // Constants
        public const string DefaultJoinValue = Constants.Strings.Space;

        // Properties
        /// <summary>
        /// String value used to join together all the values from the property values if it's a list
        /// </summary>
        public string ListJoinVlaue { get; }

        public TextListArgument(string prefix = null, string format = null, string listJoinValue = DefaultJoinValue, TextParsingOptions parsingOption = TextParsingOptions.None, bool allowEmpty = false, int order = LinuxConstants.DefaultLinuxArgumentOrder, bool required = false) : base(prefix, format, parsingOption, allowEmpty, order, required)
        {
            ListJoinVlaue = listJoinValue.ValidateArgument(nameof(listJoinValue));
        }

        public override string CreateArgument(object value = null)
        {
            if(value != null && !value.GetType().IsString() && value.GetType().IsItemContainer())
            {
                var values = new List<string>();

                foreach (var item in (IEnumerable)value)
                {
                    values.Add(ParsingOption.GetValue().FormatString(item.GetArgumentValue()));
                }

                if (!values.HasValue() && !AllowEmpty)
                {
                    return null;
                }

                var joinedValues = values.JoinString(ListJoinVlaue);

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
