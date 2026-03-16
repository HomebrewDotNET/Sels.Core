using System.Reflection;

namespace Sels.Core.Deployment.Parsing.Environment
{
    /// <summary>
    /// Attribute that defines the env variable name to parse the property value from.
    /// Used to overwrite the default naming convention.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class EnvironmentVariableNameAttribute : Attribute
    {
        // Properties
        /// <summary>
        /// The name of the environment variable to get the value from.
        /// </summary>
        public string Name { get; }

        /// <inheritdoc cref="EnvironmentVariableNameAttribute"/>
        /// <param name="name"><inheritdoc cref="Name"/></param>
        public EnvironmentVariableNameAttribute(string name)
        {
            Name = name.ValidateArgumentNotNullOrWhitespace(nameof(name));
        }
    }

    /// <summary>
    /// Contains extension methods for <see cref="EnvironmentVariableNameAttribute"/>.
    /// </summary>
    public static class EnvironmentVariableNameAttributeExtensions
    {
        /// <summary>
        /// Returns the environment variable name defined by <see cref="EnvironmentVariableNameAttribute"/>.
        /// </summary>
        /// <param name="property">The property to check the name for</param>
        /// <returns>The environment variable name or null if no attribute is defined</returns>
        public static string? GetEnvironmentVariableName(this PropertyInfo property)
        {
            property.ValidateArgument(nameof(property));

            return property.GetCustomAttribute<EnvironmentVariableNameAttribute>()?.Name;
        }
    }
}
