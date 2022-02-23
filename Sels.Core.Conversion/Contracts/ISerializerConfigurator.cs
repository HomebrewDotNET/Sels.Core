using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sels.Core.Conversion.Contracts
{
    /// <summary>
    /// Exposes common configuration for serializers.
    /// </summary>
    /// <typeparam name="TConfigurator">The type of configurator inheriting from this interface</typeparam>
    public interface ISerializerConfigurator<TConfigurator>
    {
        /// <summary>
        /// Defines extra converters that will be used to convert between the property values and the serialized strings.
        /// </summary>
        /// <param name="converters">The converters to use</param>
        /// <returns>Current configurator for method chaining</returns>
        TConfigurator UseConverters(params ITypeConverter[] converters);
        /// <summary>
        /// Defines extra filters that will be used to filter the serialized / to be deserialized strings.
        /// </summary>
        /// <param name="filters">The filters to use</param>
        /// <returns>Current configurator for method chaining</returns>
        TConfigurator UseFilters(params ISerializationFilter[] filters);
        /// <summary>
        /// Defines extra element filters that will be used to filter the serialized / to be deserialized strings.
        /// </summary>
        /// <param name="filters">The filters to use</param>
        /// <returns>Current configurator for method chaining</returns>
        TConfigurator UseElementFilters(params ISerializationFilter[] filters);
        /// <summary>
        /// Defines loggers that allows the serializer to trace.
        /// </summary>
        /// <param name="loggers">The loggers to use</param>
        /// <returns>Current configurator for method chaining</returns>
        TConfigurator UseLoggers(params ILogger[] loggers);
        /// <summary>
        /// Defines what properties will be used to serialize/deserialize.
        /// </summary>
        /// <param name="bindingFlags">The flags that tell what properties to use</param>
        /// <returns>Current configurator for method chaining</returns>
        TConfigurator ForProperties(BindingFlags bindingFlags);
    }
    /// <summary>
    /// Exposes common configuration for serializers.
    /// </summary>
    public interface ISerializerConfigurator : ISerializerConfigurator<ISerializerConfigurator>
    {

    }
}
