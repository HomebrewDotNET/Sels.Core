using Sels.Core.Deployment.Parsing.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Logging.Advanced;
using System.Reflection;
using SystemEnvironment = System.Environment;
using Sels.Core.Components.Scope;
using Sels.Core.Conversion.Converters;
using System.Text.RegularExpressions;
using Sels.Core.Conversion.Attributes.Serialization;

namespace Sels.Core.Deployment
{
    /// <summary>
    /// Contains helper methods for the deployment of applications.
    /// </summary>
    public static class Deploy
    {
        /// <summary>
        /// Contains helpers methods for working with environment variables.
        /// </summary>
        public static class Environment
        {
            /// <summary>
            /// Parses all environment variables to the properties on <typeparamref name="T"/>.
            /// </summary>
            /// <typeparam name="T">The type of the instance to parse to</typeparam>
            /// <param name="instance">The object instance to parse to</param>
            /// <param name="configurator">Optional delegate to configure the parser</param>
            /// <param name="ignoreSystemTypes">If sub properties on types starting with System or Microsoft in the namespace are ignored</param>
            /// <returns><paramref name="instance"/> with the properties deserialized from environment variables</returns>
            public static T ParseFrom<T>(T instance, Action<IEnvironmentParserConfigurator>? configurator = null, bool ignoreSystemTypes = true)
            {
                instance.ValidateArgument(nameof(instance));

                var config = new ParserConfig(x =>
                {
                    configurator(x);
                    // Ignore string by default as collection
                    x.IgnoreSubProperties(x => x.PropertyType.IsString());
                    if (ignoreSystemTypes) x.IgnoreSubProperties(x => x.PropertyType.FullName.StartsWith("System.") || x.PropertyType.FullName.StartsWith("Microsoft."));
                });
                config.Logger.Log($"Parsing environment variables to <{instance}>");
                Parse(instance, typeof(T), new List<HierarchyParent>(), config, new List<object>());
                return instance;
            }
            /// <summary>
            /// Parses all environment variables to the properties on <typeparamref name="T"/>.
            /// </summary>
            /// <typeparam name="T">The type of the instance to parse to</typeparam>
            /// <param name="configurator">Optional delegate to configure the parser</param>
            /// <param name="ignoreSystemTypes">If sub properties on types starting with System or Microsoft in the namespace are ignored</param>
            /// <returns>An instance of <typeparamref name="T"/> with the properties deserialized from environment variables</returns>
            public static T ParseFrom<T>(Action<IEnvironmentParserConfigurator>? configurator = null, bool ignoreSystemTypes = true) where T : new() => ParseFrom(new T(), configurator, ignoreSystemTypes);

            #region Helpers
            private static void Parse(object? currentInstance, Type instanceType, List<HierarchyParent> parents, ParserConfig config, List<object> alreadyChecked)
            {
                using (config.Logger.TraceMethod(typeof(Environment)))
                {
                    instanceType.ValidateArgument(nameof(instanceType));
                    alreadyChecked.ValidateArgument(nameof(alreadyChecked));
                    config.ValidateArgument(nameof(config));
                    parents.ValidateArgument(nameof(parents));

                    // Check if we already checked the instance
                    if (currentInstance != null)
                    {
                        if (alreadyChecked.Contains(currentInstance))
                        {
                            config.Logger.Debug($"Already checked instance <{currentInstance}>. Skipping");
                            return;
                        }
                        else
                        {
                            alreadyChecked.Add(currentInstance);
                        }
                    }

                    // Loop over properties and search for properties we can set
                    foreach (var property in instanceType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.CanWrite && !x.IsIgnoredForSerialization()))
                    {
                        config.Logger.Debug($"Checking property <{property.Name}> from type <{instanceType}>");
                        var envVariableName = GetEnvVariableName(config, property, parents);
                        config.Logger.Trace($"Expected env variable name for property <{property.Name}> from type <{instanceType}> is <{envVariableName}>");
                        var value = SystemEnvironment.GetEnvironmentVariable(envVariableName);

                        if (value != null)
                        {
                            config.Logger.Debug($"Env variable <{envVariableName}> has a value of <{value}>. Attempting conversion");
                            var converted = ParseValue(value, envVariableName, property.PropertyType, !config.IsIgnoredAsCollection(property), config);

                            if(converted != null)
                            {
                                SetValue(currentInstance, instanceType, property, parents, converted);
                            }
                            else
                            {
                                config.Logger.Warning($"Could not convert value <{value}> to type <{property.PropertyType}>");
                                continue;
                            }
                        }
                        else if (!config.IgnoreMultiVariableArrays && !config.IsIgnoredAsCollection(property) && property.PropertyType.IsContainer())
                        {
                            config.Logger.Debug($"Could not find env variable <{envVariableName}> but property is collection so searching for element env variables");

                            var counter = 1;
                            var elements = new List<(string Name, string Value)>();
                            while (true)
                            {
                                var elementEnvVariableName = $"{envVariableName}.{counter}";
                                var elementValue = SystemEnvironment.GetEnvironmentVariable(elementEnvVariableName);
                                if (elementValue == null)
                                {
                                    config.Logger.Debug($"Could not find element env variable <{elementEnvVariableName}>. Aborting search");
                                    break;
                                }

                                elements.Add((elementEnvVariableName, elementValue));
                            }

                            if (elements.HasValue())
                            {
                                var elementType = property.PropertyType.GetElementTypeFromCollection();
                                config.Logger.Debug($"Converting <{elements.Count}> elements to type <{elementType}>");
                                var convertedElements = new List<object>();
                                foreach (var element in elements)
                                {
                                    var converted = ParseValue(element.Value, element.Name, elementType, true, config);
                                    if(converted == null)
                                    {
                                        config.Logger.Warning($"Could not convert element <{element.Value}> to type <{elementType}>");
                                    }
                                    else
                                    {
                                        convertedElements.Add(converted);
                                    }
                                }

                                // Convert converted elements to target collection
                                var collection = GenericConverter.DefaultCollectionConverter.ConvertTo(convertedElements.CreateList(elementType), property.PropertyType);
                                SetValue(currentInstance, instanceType, property, parents, collection);
                                continue;
                            }
                        }

                        var canFallthrough = config.CanFallthrough(property);
                        config.Logger.Debug($"Could not find env variable <{envVariableName}>. {(canFallthrough ? "Going further down hierarchy by checking property type" : string.Empty)}");
                        if (!canFallthrough) continue;

                        // Check sub properties
                        var parent = new HierarchyParent(currentInstance, property);
                        using (new ScopedAction(() => parents.Add(parent), () => parents.Remove(parent)))
                        {
                            var instance = currentInstance != null ? property.GetValue(currentInstance) : null;
                            Parse(instance, instance != null ? instance.GetType() : property.PropertyType, parents, config, alreadyChecked);
                        }
                    }
                }
            }

            private static object? ParseValue(string value, string envVariableName, Type targetType, bool canParseCollection, ParserConfig config)
            {
                if (config.Converter.TryConvertTo(value, targetType, out var converted))
                {
                    config.Logger.Log($"Successfully parsed environment variable <{envVariableName}>");
                    return converted;
                }
                // Parse as array
                else if (canParseCollection && targetType.IsContainer())
                {
                    config.Logger.Debug($"Parsing value <{value}> as collection");

                    var values = value.Split(config.Splitter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var convertedValues = new List<object>();
                    var collectionType = targetType.GetElementTypeFromCollection();
                    foreach(var item in values)
                    {
                        if(config.Converter.TryConvertTo(item, collectionType, out var convertedItem))
                        {
                            convertedValues.Add(convertedItem);
                        }
                        else if (config.IgnoreConversionErrors)
                        {
                            if (!config.IgnoreConversionErrors) throw new CouldNotParseEnvironmentVariableException(value, envVariableName);
                        }
                    }
                    config.Logger.Log($"Successfully parsed environment variable <{envVariableName}>");
                    return GenericConverter.DefaultCollectionConverter.ConvertTo(convertedValues.CreateList(collectionType), targetType);
                }
                // Parse as object
                else if (targetType.IsClass)
                {
                    config.Logger.Debug($"Parsing value <{value}> as object");
                    var splitter = $"(?<!\\\\){config.Splitter}";
                    var propertyPairs = new Dictionary<string, string>();

                    // Get the name of the property to set and the value for each pair
                    foreach(var pair in Regex.Split(value, splitter).Select(x => x.Trim()))
                    {
                        if (pair.TrySplitOnFirst(':', out var property, out var propertyValue, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        {
                            propertyPairs.AddOrUpdate(property, propertyValue);
                        }
                        else if (config.IgnoreConversionErrors)
                        {
                            if (!config.IgnoreConversionErrors) throw new CouldNotParseEnvironmentVariableException(value, envVariableName);
                        }
                    }

                    var targetInstance = targetType.Construct();

                    foreach(var propertyPair in propertyPairs)
                    {
                        var property = targetType.GetProperty(propertyPair.Key);

                        if(property != null && config.Converter.TryConvertTo(propertyPair.Value, property.PropertyType, out var propertyValue))
                        {
                            property.SetValue(targetInstance, propertyValue);
                        }
                        else if (config.IgnoreConversionErrors)
                        {
                            if (!config.IgnoreConversionErrors) throw new CouldNotParseEnvironmentVariableException(value, envVariableName);
                        }
                    }

                    config.Logger.Log($"Successfully parsed environment variable <{envVariableName}>");
                    return targetInstance;
                }
                else
                {
                    config.Logger.Debug($"Could not convert element <{value}> to type <{targetType}>");
                    if (!config.IgnoreConversionErrors) throw new CouldNotParseEnvironmentVariableException(value, envVariableName);
                    return null;
                }
            }

            private static string GetEnvVariableName(ParserConfig config, PropertyInfo property, List<HierarchyParent> parents)
            {
                config.ValidateArgument(nameof(config));
                property.ValidateArgument(nameof(property));
                parents.ValidateArgument(nameof(parents));

                var name = property.GetEnvironmentVariableName();
                if (!name.HasValue()) name = parents.HasValue() ? $"{parents.Select(x => x.Property.Name).JoinString()}.{property.Name}" : property.Name;
                if (config.Prefix.HasValue()) name = $"{config.Prefix}.{name}";

                return name;
            }

            private static void SetValue(object? instance, Type instanceType, PropertyInfo property, List<HierarchyParent> parents, object valueToSet)
            {
                if (instance == null) instance = instanceType.Construct();
                property.SetValue(instance, valueToSet);

                // Go through hierarchy and make sure all parents have an instance
                if (parents.HasValue())
                {
                    HierarchyParent lastParent = null;
                    foreach (var parent in parents)
                    {
                        // Skip already created instances
                        if (parent.Instance != null)
                        {
                            lastParent = parent;
                            continue;
                        };

                        // Create instance by checking the property for the type to contruct
                        var typeToContruct = lastParent.Property.PropertyType;
                        var parentInstance = typeToContruct.Construct();

                        // Set property value on the parent so the hierarchy is linked
                        parent.Instance = parentInstance;
                        lastParent.Property.SetValue(lastParent.Instance, parentInstance);
                        lastParent = parent;
                    }

                    // Set instance on last parent
                    lastParent.Property.SetValue(lastParent.Instance, instance);
                }
            }

            private class HierarchyParent
            {
                public HierarchyParent()
                {

                }

                public HierarchyParent(object? instance, PropertyInfo property)
                {
                    Instance = instance;
                    Property = property;
                }

                public object? Instance { get; set; }
                public PropertyInfo Property { get; set; }
            }
            #endregion
        }
    }
}
