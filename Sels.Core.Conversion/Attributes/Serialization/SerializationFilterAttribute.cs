using Sels.Core.Conversion;
using Sels.Core.Conversion.Serialization.Filters;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sels.Core.Conversion.Attributes.Serialization
{
    /// <summary>
    /// Defines a custom filter for serializers that support it. Filters defined on class level are inherited by properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class SerializationFilterAttribute : Attribute
    {
        // Properties
        /// <summary>
        /// The filter to use for serialized and to be deserialized values.
        /// </summary>
        public ISerializationFilter Filter { get; }
        /// <summary>
        /// If this filter should be used on the element substrings instead of the whole string that contains the element substrings.
        /// </summary>
        public bool IsElementFilter { get; set; }

        /// <summary>
        /// Defines a custom filter for serializers that support it. Filters defined on class level are inherited by properties.
        /// </summary>
        /// <param name="filterType">The type of the filter to use</param>
        /// <param name="arguments">Optional constructor parameters for <paramref name="filterType"/></param>
        public SerializationFilterAttribute(Type filterType, params object[] arguments)
        {
            filterType.ValidateArgumentAssignableTo(nameof(filterType), typeof(ISerializationFilter));
            filterType.ValidateArgumentCanBeContructedWithArguments(nameof(filterType), arguments);

            Filter = filterType.Construct<ISerializationFilter>(arguments);
        }
    }

    /// <summary>
    /// Contains extension methods for working with <see cref="SerializationFilterAttribute"/>.
    /// </summary>
    public static class SerializationFilterExtensions
    {
        /// <summary>
        /// Returns all filters defined on the type of <paramref name="source"/> by looking for <see cref="SerializationFilterAttribute"/>.
        /// </summary>
        /// <param name="source">The object to get the filters for</param>
        /// <param name="isElementFilter">When set to true it will only fetch filters for elements</param>
        /// <returns>All filters defined by <see cref="SerializationFilterAttribute"/> or an empty array if no attributes are defined</returns>
        public static ISerializationFilter[] GetFilters(this object source, bool isElementFilter = false)
        {
            source.ValidateArgument(nameof(source));

            return GetFilters(source.GetType(), isElementFilter);
        }
        /// <summary>
        /// Returns all filters defined on <paramref name="source"/> by looking for <see cref="SerializationFilterAttribute"/>.
        /// </summary>
        /// <param name="source">The type to get the filters from</param>
        /// <param name="isElementFilter">When set to true it will only fetch filters for elements</param>
        /// <returns>All filters defined by <see cref="SerializationFilterAttribute"/> or an empty array if no attributes are defined</returns>
        public static ISerializationFilter[] GetFilters(this Type source, bool isElementFilter = false)
        {
            source.ValidateArgument(nameof(source));

            return source.GetCustomAttributes<SerializationFilterAttribute>().Where(x => x.IsElementFilter == isElementFilter).Select(x => x.Filter).ToArray();
        }
        /// <summary>
        /// Returns all filters defined on <paramref name="source"/> by looking for <see cref="SerializationFilterAttribute"/>.
        /// </summary>
        /// <param name="source">The member to get the filters from</param>
        /// <param name="isElementFilter">When set to true it will only fetch filters for elements</param>
        /// <returns>All filters defined by <see cref="SerializationFilterAttribute"/> or an empty array if no attributes are defined</returns>
        public static ISerializationFilter[] GetFilters(this MemberInfo source, bool isElementFilter = false)
        {
            source.ValidateArgument(nameof(source));

            return source.GetCustomAttributes<SerializationFilterAttribute>().Where(x => x.IsElementFilter == isElementFilter).Select(x => x.Filter).ToArray();
        }
    }
}
