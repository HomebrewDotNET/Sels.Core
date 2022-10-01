using Sels.Core.Extensions.Conversion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Extensions.Reflection
{
    /// <summary>
    /// Generic extension methods for creating instances of types through reflection.
    /// </summary>
    public static class ActivationExtensions
    {
        /// <summary>
        /// Creates a generic <see cref="List{T}"/> of type <paramref name="listType"/> and adds all elements in <paramref name="source"/> if it not null.
        /// </summary>
        /// <param name="source">Enumerable with elelemts to add.</param>
        /// <param name="listType">The type for the list to create</param>
        /// <returns>A generic <see cref="List{T}"/> of type <paramref name="listType"/> with all types from <paramref name="source"/></returns>
        public static IList CreateList(this IEnumerable source, Type listType)
        {
            listType.ValidateArgument(nameof(listType));
            var list = typeof(List<>).MakeGenericType(listType).Construct().Cast<IList>();

            if(source != null)
            {
                foreach(var item in source)
                {
                    list.Add(item);
                }
            }

            return list;
        }

        /// <summary>
        /// Creates a generic <see cref="List{T}"/> from <paramref name="elementType"/>.
        /// </summary>
        /// <param name="elementType">The generic type for the list to create</param>
        /// <returns>A generic <see cref="List{T}"/> of type <paramref name="elementType"/></returns>
        public static IList CreateList(this Type elementType)
        {
            elementType.ValidateArgument(nameof(elementType));
            var list = typeof(List<>).MakeGenericType(elementType).Construct().Cast<IList>();

            return list;
        }
    }
}
