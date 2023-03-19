using Sels.Core.Extensions;

namespace Sels.Core.Components.Reflection.Searching
{
    /// <summary>
    /// Contains extension methods for <see cref="Seeker{T}"/>.
    /// </summary>
    public static class SeekerExtensions
    {
        /// <summary>
        /// Seeker will ignore all properties that have types that start with System. in the namespace.
        /// </summary>
        /// <typeparam name="T">Generic type of <paramref name="seeker"/></typeparam>
        /// <param name="seeker">The seeker to configure</param>
        /// <returns><paramref name="seeker"/> for method chaining</returns>
        public static Seeker<T> IgnoreSystemTypes<T>(this Seeker<T> seeker)
        {
            seeker.ValidateArgument(nameof(seeker));

            return seeker.IgnoreForFallThrough((source, property, value) => property.PropertyType.FullName.StartsWith("System."));
        }

        /// <summary>
        /// Seeker will ignore all properties that have types that start with Microsoft. in the namespace.
        /// </summary>
        /// <typeparam name="T">Generic type of <paramref name="seeker"/></typeparam>
        /// <param name="seeker">The seeker to configure</param>
        /// <returns><paramref name="seeker"/> for method chaining</returns>
        public static Seeker<T> IgnoreMicrosoftTypes<T>(this Seeker<T> seeker)
        {
            seeker.ValidateArgument(nameof(seeker));

            return seeker.IgnoreForFallThrough((source, property, value) => property.PropertyType.FullName.StartsWith("Microsoft."));
        }
    }
}
