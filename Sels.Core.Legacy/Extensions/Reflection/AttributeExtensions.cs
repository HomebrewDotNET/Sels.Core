using System;
using System.Reflection;

namespace Sels.Core.Extensions.Reflection
{
    /// <summary>
    /// Contains extension methods for working with attributes.
    /// </summary>
    public static class AttributeExtensions
    {
        /// <summary>
        /// Gets the first attribute of type <typeparamref name="T"/> from the type of <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of attribute to get</typeparam>
        /// <param name="source">The object to get the attribute from</param>
        /// <returns>The first attribute of type <typeparamref name="T"/></returns>
        /// <exception cref="InvalidOperationException">Thrown when the attribute could not be found</exception>
        public static T GetAttribute<T>(this object source) where T : Attribute
        {
            source.ValidateArgument(nameof(source));

            var attribute = source.GetAttributeOrDefault<T>();

            if (!attribute.HasValue())
            {
                throw new InvalidOperationException($"Attribute {typeof(T)} was not present on object {source.GetType()}");
            }

            return attribute;
        }

        /// <summary>
        /// Gets the first attribute of type <typeparamref name="T"/> from member <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of attribute to get</typeparam>
        /// <param name="source">The member to get the attribute from</param>
        /// <returns>The first attribute of type <typeparamref name="T"/></returns>
        /// <exception cref="InvalidOperationException">Thrown when the attribute could not be found</exception>
        public static T GetAttribute<T>(this MemberInfo source) where T : Attribute
        {
            source.ValidateArgument(nameof(source));

            var attribute = source.GetAttributeOrDefault<T>();

            if (!attribute.HasValue())
            {
                throw new InvalidOperationException($"Attribute {typeof(T)} was not present on object {source.GetType()}");
            }

            return attribute;
        }
        /// <summary>
        /// Gets the first attribute of type <typeparamref name="T"/> from enum value <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of attribute to get</typeparam>
        /// <param name="source">The enum value to get the attribute from</param>
        /// <returns>The first attribute of type <typeparamref name="T"/></returns>
        /// <exception cref="InvalidOperationException">Thrown when the attribute could not be found</exception>
        public static T GetAttribute<T>(this Enum source) where T : Attribute
        {
            source.ValidateArgument(nameof(source));

            var attribute = source.GetAttributeOrDefault<T>();

            if (!attribute.HasValue())
            {
                throw new InvalidOperationException($"Attribute {typeof(T)} was not present on object {source.GetType()}");
            }

            return attribute;
        }

        /// <summary>
        /// Tries to get the first attribute of type <typeparamref name="T"/> from the type of <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of attribute to get</typeparam>
        /// <param name="source">The object to get the attribute from</param>
        /// <returns>The first attribute of type <typeparamref name="T"/> or null if none are found</returns>
        public static T GetAttributeOrDefault<T>(this object source) where T : Attribute
        {
            source.ValidateArgument(nameof(source));

            var sourceType = source.GetType();

            return sourceType.GetCustomAttribute<T>();
        }

        /// <summary>
        /// Tries to get the first attribute of type <typeparamref name="T"/> from member <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of attribute to get</typeparam>
        /// <param name="source">The member to get the attribute from</param>
        /// <returns>The first attribute of type <typeparamref name="T"/> or null if none are found</returns>
        public static T GetAttributeOrDefault<T>(this MemberInfo source) where T : Attribute
        {
            source.ValidateArgument(nameof(source));

            return source.GetCustomAttribute<T>();
        }

        /// <summary>
        /// Tries to get the first attribute of type <typeparamref name="T"/> from enum value <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of attribute to get</typeparam>
        /// <param name="source">The enum value to get the attribute from</param>
        /// <returns>The first attribute of type <typeparamref name="T"/> or null if none are found</returns>
        public static T GetAttributeOrDefault<T>(this Enum source) where T : Attribute
        {
            source.ValidateArgument(nameof(source));

            var sourceType = source.GetType();

            var enumMember = sourceType.GetMember(source.ToString());

            return enumMember[0].GetCustomAttribute<T>();
        }
    }
}
