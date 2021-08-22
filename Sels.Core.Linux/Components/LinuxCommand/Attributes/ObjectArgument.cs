using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Linux.Extensions.Argument;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Attributes
{
    /// <summary>
    /// Creates argument by calling a property getter, field or method on the property. Uses <see cref="object.ToString()"/> or the value defined in the <see cref="LinuxValueAttribute"/>.
    /// </summary>
    public class ObjectArgument : TextArgument
    {
        // Properties
        private Selector Selector { get; }
        private string Target { get; }
        private object Argument { get; set; }

        /// <summary>
        /// Defines an argument whose value will be created by calling a property getter, field or method on the property. Uses <see cref="object.ToString()"/> or the value defined in the <see cref="LinuxValueAttribute"/>.
        /// </summary>
        /// <param name="selector">What member type to get argument value from</param>
        /// <param name="target">Where to get argument value from</param>
        /// <param name="argument">Optional argument for method</param>
        /// <param name="prefix">Optional prefix that will be placed along side the property value based on <paramref name="format"/></param>
        /// <param name="format">How the <paramref name="prefix"/> and property value should be formatted. Use <see cref="PrefixFormat"/> for the <paramref name="prefix"/> and <see cref="ValueFormat"/> for the property value</param>
        /// <param name="parsingOption">Optional parsing for the property value</param>
        /// <param name="order">Used to order argument. Lower means it will get placed in the argument list first. Negative gets placed last in the argument list.</param>
        /// <param name="required">Indicates if this property must be set. Throws InvalidOperation when Required is true but property value is null.</param>
        public ObjectArgument(Selector selector, string target, object argument = null, string prefix = null, string format = null, TextParsingOptions parsingOption = TextParsingOptions.None, int order = LinuxConstants.DefaultLinuxArgumentOrder, bool required = false) : base(prefix, format, parsingOption, false, false, order, required)
        {
            Selector = selector;
            Target = target.ValidateArgumentNotNullOrWhitespace(nameof(target));
            Argument = argument;
        }

        public override string CreateArgument(object value = null)
        {
            if (value.HasValue())
            {
                object argument = null;
                var type = value.GetType();

                switch (Selector)
                {
                    case Selector.Field:
                        var field = type.GetFields().FirstOrDefault(x => x.Name.Equals(Target, StringComparison.OrdinalIgnoreCase));
                        field.ValidateArgument(x => x.HasValue(), x => new InvalidOperationException($"Could not find field with name <{Target}> on type <{type}>"));
                        argument = field.GetValue(value);
                        break;
                    case Selector.Property:
                        var property = type.GetProperties().FirstOrDefault(x => x.Name.Equals(Target, StringComparison.OrdinalIgnoreCase) && x.CanRead);
                        property.ValidateArgument(x => x.HasValue(), x => new InvalidOperationException($"Could not find property with name <{Target}> on type <{type}> that can be read"));
                        argument = property.GetValue(value);
                        break;

                    case Selector.Method:
                        if (Argument.HasValue())
                        {
                            var method = type.GetMethods().FirstOrDefault(x => x.Name.Equals(Target, StringComparison.OrdinalIgnoreCase) && x.GetParameters().Length == 1 && x.GetParameters()[0].GetType().IsAssignableFrom(Argument.GetType()));
                            method.ValidateArgument(x => x.HasValue(), x => new InvalidOperationException($"Could not find method with name <{Target}> on type <{type}> that has a parameter of type <{Argument.GetType()}>"));
                            argument = method.Invoke(value, Argument.AsArray());
                        }
                        else
                        {
                            var method = type.GetMethods().FirstOrDefault(x => x.Name.Equals(Target, StringComparison.OrdinalIgnoreCase) && x.GetParameters().Length == 0);
                            method.ValidateArgument(x => x.HasValue(), x => new InvalidOperationException($"Could not find method with name <{Target}> on type <{type}> without any parameters"));
                            argument = method.Invoke(value, null);
                        }
                        break;
                }

                return base.CreateArgument(argument);
            }

            return string.Empty;
        }
    }

    public enum Selector
    {
        /// <summary>
        /// Get value from a public field.
        /// </summary>
        Field,
        /// <summary>
        /// Get Value from a public property.
        /// </summary>
        Property,
        /// <summary>
        /// Get value from calling a method.
        /// </summary>
        Method
    }
}
