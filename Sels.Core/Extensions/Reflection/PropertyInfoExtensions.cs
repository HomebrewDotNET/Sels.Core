using Sels.Core.Extensions.Conversion;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sels.Core.Extensions.Reflection
{
    /// <summary>
    /// Contains extension methods for working with <see cref="PropertyInfo"/>.
    /// </summary>
    public static class PropertyInfoExtensions
    {
        /// <summary>
        /// Gets all property info defined on the type of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The object to get the properties from</param>
        /// <returns>The properties on <paramref name="value"/></returns>
        public static PropertyInfo[] GetProperties(this object value)
        {
            value.ValidateArgument(nameof(value));

            return value.GetType().GetProperties();
        }

        /// <summary>
        /// Gets all property info defined on the type of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The object to get the properties from</param>
        /// <param name="bindingFlags">Defines what properties to return</param>
        /// <returns>The properties on <paramref name="value"/></returns>
        public static PropertyInfo[] GetProperties(this object value, BindingFlags bindingFlags)
        {
            value.ValidateArgument(nameof(value));

            return value.GetType().GetProperties(bindingFlags);
        }

        /// <summary>
        /// Gets all public instance and static properties on the type of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The object to get the properties from</param>
        /// <returns>The public instance and static properties on <paramref name="value"/></returns>
        public static PropertyInfo[] GetPublicProperties(this object value)
        {
            return value.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>
        /// Gets all public instance and static properties on <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to get the properties from</param>
        /// <returns>The public instance and static properties on <paramref name="type"/></returns>
        public static PropertyInfo[] GetPublicProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>
        /// Checks if <paramref name="property"/> is the same property as <paramref name="propertyToCompare"/>.
        /// </summary>
        /// <param name="property">The first property to compare</param>
        /// <param name="propertyToCompare">The second property to compare</param>
        /// <returns>Whether or not <paramref name="property"/> is equal to <paramref name="propertyToCompare"/></returns>
        public static bool AreEqual(this PropertyInfo property, PropertyInfo propertyToCompare)
        {
            property.ValidateArgument(nameof(property));
            propertyToCompare.ValidateArgument(nameof(propertyToCompare));

            if (property.Equals(propertyToCompare) || (property.Name.Equals(propertyToCompare.Name) && property.DeclaringType.Equals(propertyToCompare.DeclaringType)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the value of property <paramref name="property"/> on instance <paramref name="sourceObject"/> and casts it to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the property value</typeparam>
        /// <param name="property">The property to get the value from</param>
        /// <param name="sourceObject">The instance to get the value from</param>
        /// <returns>The value from <paramref name="property"/> on <paramref name="sourceObject"/></returns>
        public static T GetValue<T>(this PropertyInfo property, object sourceObject)
        {
            property.ValidateArgument(nameof(property));
            sourceObject.ValidateArgument(nameof(sourceObject));

            return property.GetValue(sourceObject).Cast<T>();
        }

        /// <summary>
        /// Checks if values of type <typeparamref name="T"/> can be assigned to property <paramref name="property"/>.
        /// </summary>
        /// <typeparam name="T">The type to check</typeparam>
        /// <param name="property">The property to check</param>
        /// <returns>If values of type <typeparamref name="T"/> can be assigned to property <paramref name="property"/></returns>
        public static bool CanAssign<T>(this PropertyInfo property)
        {
            property.ValidateArgument(nameof(property));
            var typeToCheck = typeof(T);
            var propertyType = property.PropertyType;

            return property.CanWrite && propertyType.IsAssignableFrom(typeToCheck);
        }

        /// <summary>
        /// Tries to extract the property with name <paramref name="propertyName"/> from the type of <paramref name="sourceObject"/>.
        /// </summary>
        /// <param name="sourceObject">The object to get the property from</param>
        /// <param name="propertyName">The name of the property to search for</param>
        /// <param name="property">The property info if the property was found</param>
        /// <returns>Whether or not a property was found</returns>
        public static bool TryGetPropertyInfo(this object sourceObject, string propertyName, out PropertyInfo property)
        {
            sourceObject.ValidateArgument(nameof(sourceObject));
            propertyName.ValidateArgumentNotNullOrWhitespace(nameof(propertyName));

            property = sourceObject.GetType().GetProperty(propertyName);

            return property != null;
        }
        /// <summary>
        /// Tries to extract the property with name <paramref name="propertyName"/> from <paramref name="sourceType"/>.
        /// </summary>
        /// <param name="sourceType">The type to get the property from</param>
        /// <param name="propertyName">The name of the property to search for</param>
        /// <param name="property">The property info if the property was found</param>
        /// <returns>Whether or not a property was found</returns>
        public static bool TryGetPropertyInfo(this Type sourceType, string propertyName, out PropertyInfo property)
        {
            sourceType.ValidateArgument(nameof(sourceType));
            propertyName.ValidateArgument(nameof(propertyName));

            property = sourceType.GetProperty(propertyName);

            return property != null;
        }

        /// <summary>
        /// Gets the property with name <paramref name="propertyName"/> from the type of <paramref name="sourceObject"/>.
        /// </summary>
        /// <param name="sourceObject">The object to get the property from</param>
        /// <param name="propertyName">The name of the property to search for</param>
        /// <returns>The property with name <paramref name="propertyName"/></returns>
        /// <exception cref="InvalidOperationException">Thrown when the property could not be found</exception>
        public static PropertyInfo GetPropertyInfo(this object sourceObject, string propertyName)
        {
            sourceObject.ValidateArgument(nameof(sourceObject));
            propertyName.ValidateArgument(nameof(propertyName));

            if (sourceObject.TryGetPropertyInfo(propertyName, out var property))
            {
                return property;
            }
            else
            {
                throw new InvalidOperationException($"Could not find property <{propertyName}> on object <{sourceObject.GetType()}>");
            }
        }

        /// <summary>
        /// Gets the property with name <paramref name="propertyName"/> from <paramref name="sourceType"/>.
        /// </summary>
        /// <param name="sourceType">The type to get the property from</param>
        /// <param name="propertyName">The name of the property to search for</param>
        /// <returns>The property with name <paramref name="propertyName"/></returns>
        /// <exception cref="InvalidOperationException">Thrown when the property could not be found</exception>
        public static PropertyInfo GetPropertyInfo(this Type sourceType, string propertyName)
        {
            sourceType.ValidateArgument(nameof(sourceType));
            propertyName.ValidateArgument(nameof(propertyName));

            if (sourceType.TryGetPropertyInfo(propertyName, out var property))
            {
                return property;
            }
            else
            {
                throw new InvalidOperationException($"Could not find property <{propertyName}> on type <{sourceType}>");
            }
        }

        /// <summary>
        /// Sets the default value of the property type on property <paramref name="property"/> on instance <paramref name="sourceObject"/>.
        /// </summary>
        /// <param name="property">The property to set</param>
        /// <param name="sourceObject">The instance to set the value on</param>
        public static void SetDefault(this PropertyInfo property, object sourceObject)
        {
            property.ValidateArgument(nameof(property));
            sourceObject.ValidateArgument(nameof(sourceObject));

            var propertyType = property.PropertyType;

            var defaultValue = propertyType.GetDefaultValue();

            property.SetValue(sourceObject, defaultValue);
        }
    }
}
