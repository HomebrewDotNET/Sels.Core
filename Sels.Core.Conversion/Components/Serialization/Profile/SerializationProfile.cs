using Microsoft.Extensions.Logging;
using Sels.Core.Conversion.Attributes.Serialization;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Serialization.Filters;

namespace Sels.Core.Conversion.Components.Serialization.Profile
{
    /// <summary>
    /// Builds a serialization profile for a type by creating <see cref="PropertySerializationProfile"/> profiles for all properties that can be serialized.
    /// </summary>
    public class SerializationProfile
    {
        // Properties
        /// <summary>
        /// The type the profile is for.
        /// </summary>
        public Type Type { get; }
        /// <summary>
        /// Dictionary with properties that can be serialized together with their profile.
        /// </summary>
        public Dictionary<PropertyInfo, PropertySerializationProfile> PropertyProfiles { get; } = new Dictionary<PropertyInfo, PropertySerializationProfile>();

        /// <inheritdoc cref="SerializationProfile"/>
        /// <param name="type">The type to create the profile for</param>
        /// <param name="propertyFlags">The binding flags that dictate what properties to include in this profile</param>
        /// <param name="additionalConverters">Optional extra converters for the property profiles. Converters from <see cref="ConverterAttribute"/> defined on the type take priority</param>
        /// <param name="additionalFilters">Optional extra filters for the property profile. Filters from <see cref="SerializationFilterAttribute"/> defined on the type take priority</param>
        /// <param name="additionalElementFilters">Optional extra element filters for the property profile. Filters from <see cref="SerializationFilterAttribute"/> defined on the type take priority</param>
        /// <param name="loggers">Optional loggers for tracing</param>
        public SerializationProfile(Type type, BindingFlags propertyFlags = BindingFlags.Instance | BindingFlags.Public, IEnumerable<ITypeConverter> additionalConverters = null, IEnumerable<ISerializationFilter> additionalFilters = null, IEnumerable<ISerializationFilter> additionalElementFilters = null, IEnumerable<ILogger> loggers = null)
        {
            Type = type.ValidateArgument(nameof(type));

            var converters = Helper.Collection.EnumerateAll(type.GetConverters(), additionalConverters).Where(x => x != null);
            var filters = Helper.Collection.EnumerateAll(type.GetFilters(false), additionalFilters).Where(x => x != null);
            var elementFilters = Helper.Collection.EnumerateAll(type.GetFilters(true), additionalElementFilters).Where(x => x != null);

            foreach (var property in type.GetProperties(propertyFlags).Where(x => !x.IsIgnoredForSerialization()))
            {
                var profile = new PropertySerializationProfile(property, converters, filters, elementFilters, loggers);
                PropertyProfiles.Add(property, profile);
            }
        }
    }
}
