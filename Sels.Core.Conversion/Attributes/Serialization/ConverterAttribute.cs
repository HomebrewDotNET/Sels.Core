using Sels.Core.Conversion;
using Sels.Core.Conversion.Converters;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sels.Core.Conversion.Attributes.Serialization
{
    /// <summary>
    /// Defines a custom type converter for serializers that support it. Converters defined on class level are inherited by properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class ConverterAttribute : Attribute
    {
        // Properties
        /// <summary>
        /// The converter instance that will be used during serialization/deserialization.
        /// </summary>
        public ITypeConverter Converter { get; }

        /// <summary>
        /// Defines a custom type converter for serializers that support it. Converters defined on class level are inherited by properties.
        /// </summary>
        /// <param name="converterType">The type of the converter to use</param>
        /// <param name="arguments">Optional constructor parameters for <paramref name="converterType"/></param>
        public ConverterAttribute(Type converterType, params object[] arguments)
        {
            converterType.ValidateArgumentAssignableTo(nameof(converterType), typeof(ITypeConverter));
            converterType.ValidateArgumentCanBeContructedWithArguments(nameof(converterType), arguments);

            Converter = converterType.Construct<ITypeConverter>(arguments);
        }
    }
    /// <summary>
    /// Contains extension methods for working with <see cref="ConverterAttribute"/>.
    /// </summary>
    public static class ConverterAttributeExtensions
    {
        /// <summary>
        /// Returns all converters defined on the type of <paramref name="source"/> by looking for <see cref="ConverterAttribute"/>.
        /// </summary>
        /// <param name="source">The object to get the converters for</param>
        /// <returns>All converters defined by <see cref="ConverterAttribute"/> or an empty array if no attributes are defined</returns>
        public static ITypeConverter[] GetConverters(this object source)
        {
            source.ValidateArgument(nameof(source));

            return GetConverters(source.GetType());
        }
        /// <summary>
        /// Returns all converters defined on <paramref name="source"/> by looking for <see cref="ConverterAttribute"/>.
        /// </summary>
        /// <param name="source">The type to get the converters from</param>
        /// <returns>All converters defined by <see cref="ConverterAttribute"/> or an empty array if no attributes are defined</returns>
        public static ITypeConverter[] GetConverters(this Type source)
        {
            source.ValidateArgument(nameof(source));

            return source.GetCustomAttributes<ConverterAttribute>().SelectOrDefault(x => x.Converter).ToArrayOrDefault();
        }
        /// <summary>
        /// Returns all converters defined on <paramref name="source"/> by looking for <see cref="ConverterAttribute"/>.
        /// </summary>
        /// <param name="source">The member to get the converters from</param>
        /// <returns>All converters defined by <see cref="ConverterAttribute"/> or an empty array if no attributes are defined</returns>
        public static ITypeConverter[] GetConverters(this MemberInfo source)
        {
            source.ValidateArgument(nameof(source));

            return source.GetCustomAttributes<ConverterAttribute>().SelectOrDefault(x => x.Converter).ToArrayOrDefault();
        }
    }
}
