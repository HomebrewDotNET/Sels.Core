using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using Sels.Core.Conversion.Converters;
using Sels.Core.Extensions.Text;

namespace Sels.Core.Conversion.Attributes.Serialization
{
    /// <summary>
    /// Defines arguments that will be supplied to the <see cref="ITypeConverter"/> by serializers that support it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ConverterArgumentAttribute : Attribute
    {
        // Constants
        /// <summary>
        /// The substring that will split up the arguments into key/value pairs.
        /// </summary>
        public const string KeyValueSplitter = ":";

        // Properties
        /// <summary>
        /// Arguments for the type converters.
        /// </summary>
        public IReadOnlyDictionary<string, object> Arguments { get; }

        /// <summary>
        /// Defines arguments that will be supplied to the <see cref="ITypeConverter"/> by serializers that support it.
        /// </summary>
        /// <param name="arguments">List of arguments. Must use <see cref="KeyValueSplitter"/> to split the argument key from the argument value</param>
        public ConverterArgumentAttribute(params string[] arguments)
        {
            arguments.ValidateArgumentNotNullOrEmpty(nameof(arguments));
            arguments.Execute((i, x) => x.ValidateArgument(a => a.HasValue() && a.Contains(KeyValueSplitter), $"Argument {i} cannot be null, empty or whitespace and must contain <{KeyValueSplitter}>"));

            var dictionary = new Dictionary<string, object>();

            arguments.Execute(x => { var key = x.SplitOnFirst(KeyValueSplitter, out var value); dictionary.Add(key.Trim(), value.Trim()); });
            Arguments = dictionary;
        }
    }
    /// <summary>
    /// Contains extension methods for working with <see cref="ConverterArgumentAttribute"/>.
    /// </summary>
    public static class ConverterArgumentAttributeExtensions
    {
        /// <summary>
        /// Returns <see cref="ITypeConverter"/> arguments by looking for <see cref="ConverterArgumentAttribute"/>.
        /// </summary>
        /// <param name="source">The member to get the arguments from</param>
        /// <returns>The arguments defined by <see cref="ConverterArgumentAttribute"/> or null if no attribute is found</returns>
        public static IReadOnlyDictionary<string, object> GetConverterArguments(this MemberInfo source)
        {
            source.ValidateArgument(nameof(source));

            return source.GetCustomAttribute<ConverterArgumentAttribute>()?.Arguments;
        }
    }
}
