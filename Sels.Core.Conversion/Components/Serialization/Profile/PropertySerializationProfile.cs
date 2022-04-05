using Microsoft.Extensions.Logging;
using Sels.Core.Conversion.Attributes.Serialization;
using Sels.Core.Conversion;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Serialization.Filters;

namespace Sels.Core.Conversion.Components.Serialization.Profile
{
    /// <summary>
    /// Builds a serialization profile for a <see cref="PropertyInfo"/> containing classes that dictate how the property value should be serialized. Exposes some simple methods for serialization and deserialization.
    /// </summary>
    public class PropertySerializationProfile
    {
        // Fields
        private readonly IEnumerable<ILogger> _loggers;

        // Properties
        /// <summary>
        /// The property the profile was built for.
        /// </summary>
        public PropertyInfo Property { get; }
        /// <summary>
        /// Any custom type converters defined for this property. Is used for converting between the property value and the serialized string.
        /// </summary>
        public ITypeConverter[] Converters { get; }
        /// <summary>
        /// Optional arguments for <see cref="Converters"/>.
        /// </summary>
        public IDictionary<string,string> ConverterArguments { get; }
        /// <summary>
        /// Any custom filters defined for modifying the string value after serialization or before serialization.
        /// </summary>
        public ISerializationFilter[] Filters { get; }
        /// <summary>
        /// Any custom filters defined for the element substrings for modifying the string value after serialization or before serialization.
        /// </summary>
        public ISerializationFilter[] ElementFilters { get; set; }
        /// <summary>
        /// Optional collection splitter for splitting a string into multiple substrings so they can be converted to elements for a collection.
        /// </summary>
        public ElementSeparatorAttribute ElementSeparator { get; }
        /// <summary>
        /// Whether or not <see cref="Property"/> is a collection type.
        /// </summary>
        public bool IsCollection => Property.PropertyType.IsContainer();
        /// <summary>
        /// Whether or not <see cref="Property"/> is static.
        /// </summary>
        public bool IsStatic => Property.GetAccessors(true).Any(x => x.IsStatic);

        /// <summary>
        /// Builds a serialization profile for a <see cref="PropertyInfo"/> containing classes that dictate how the property value should be serialized.
        /// </summary>
        /// <param name="property">The property to build the profile for</param>
        /// <param name="additionalConverters">Optional extra converters for this property. Converters from <see cref="ConverterAttribute"/> take priority</param>
        /// <param name="additionalFilters">Optional extra filters for this property. Filters from <see cref="SerializationFilterAttribute"/> take priority</param>
        /// <param name="additionalElementFilters">Optional extra element filters for this property. Filters from <see cref="SerializationFilterAttribute"/> take priority</param>
        /// <param name="loggers">Optional loggers for tracing</param>
        public PropertySerializationProfile(PropertyInfo property, IEnumerable<ITypeConverter> additionalConverters = null, IEnumerable<ISerializationFilter> additionalFilters = null, IEnumerable<ISerializationFilter> additionalElementFilters = null, IEnumerable<ILogger> loggers = null)
        {
            Property = property.ValidateArgument(nameof(property));
            Converters = Helper.Collection.EnumerateAll(property.GetConverters(), additionalConverters).ToArray();
            ConverterArguments = property.GetConverterArguments();
            Filters = Helper.Collection.EnumerateAll(property.GetFilters(false), additionalFilters).ToArray();
            ElementFilters = Helper.Collection.EnumerateAll(property.GetFilters(true), additionalElementFilters).ToArray();
            ElementSeparator = property.GetAttributeOrDefault<ElementSeparatorAttribute>();

            _loggers = loggers;
        }

        /// <summary>
        /// Filters <paramref name="source"/> using <see cref="Filters"/>.
        /// </summary>
        /// <param name="source">The string to filter</param>
        /// <param name="isWrite">If the string needs to be filtered for write, otherwise the string will be filtered for read</param>
        /// <returns>The filtered string or <paramref name="source"/> if no filters are defined</returns>
        public string Filter(string source, bool isWrite = false)
        {
            using (_loggers.TraceMethod(this))
            {
                source.ValidateArgument(nameof(source));

                if (Filters.HasValue())
                {
                    if (isWrite)
                    {
                        _loggers.Debug($"Filtering serialized string <{source}>");
                        var filtered = Filters.Filter(source, (f, x) => f.ModifyOnWrite(x));
                        _loggers.Debug($"Filtered serialized string <{source}> to <{filtered}>");
                        return filtered;
                    }
                    else
                    {
                        _loggers.Debug($"Filtering string <{source}> that will be deserialized");
                        var filtered = Filters.Filter(source, (f, x) => f.ModifyOnRead(x));
                        _loggers.Debug($"Filtered string <{source}> to <{filtered}> that will be deserialized");
                        return filtered;
                    }
                }
                else
                {
                    _loggers.Debug($"No filters defined for property <{Property.Name}>. Skipping.");
                    return source;
                }               
            }
        }

        /// <summary>
        /// Filters all elements in <paramref name="source"/> using <see cref="ElementFilters"/>.
        /// </summary>
        /// <param name="source">The elements to filer</param>
        /// <param name="isWrite">If the string needs to be filtered for write, otherwise the string will be filtered for read</param>
        /// <returns>Enumerator returning the filtered strings or the source strings if no filters are defined</returns>
        public IEnumerable<string> Filter(IEnumerable<string> source, bool isWrite = false)
        {
            using (_loggers.TraceMethod(this))
            {
                source.ValidateArgument(nameof(source));

                if (ElementFilters.HasValue())
                {
                    foreach(var element in source)
                    {
                        var counter = 0;
                        if(element == null)
                        {
                            _loggers.Warning($"Element <{counter}> was null, skipping");
                            continue;
                        }

                        if (isWrite)
                        {
                            _loggers.Debug($"Filtering serialized element <{element}>({counter})");
                            var filtered = Filters.Filter(element, (f, x) => f.ModifyOnWrite(x));
                            _loggers.Debug($"Filtered serialized element <{element}>({counter}) to <{filtered}>");
                            yield return filtered;
                        }
                        else
                        {
                            _loggers.Debug($"Filtering element <{element}>({counter}) that will be deserialized");
                            var filtered = Filters.Filter(element, (f, x) => f.ModifyOnRead(x));
                            _loggers.Debug($"Filtered element <{element}>({counter}) to <{filtered}> that will be deserialized");
                            yield return filtered;
                        }

                        counter++;
                    }    
                }
                else
                {
                    _loggers.Debug($"No element filters defined for property <{Property.Name}>. Skipping.");
                    foreach(var element in source.Where(x => x != null))
                    {
                        yield return element;
                    }
                }
            }
        }

    }
}
