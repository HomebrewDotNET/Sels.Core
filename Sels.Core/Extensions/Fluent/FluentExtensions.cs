using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Extensions.Fluent
{
    /// <summary>
    /// Contains static extension methods for working with api's that use a fluent syntax.
    /// </summary>
    public static class FluentExtensions
    {
        #region ForEach
        /// <summary>
        /// Executes <paramref name="action"/> for each item in <paramref name="items"/> on <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of the fluent object</typeparam>
        /// <typeparam name="TItem">Type of the items to perform the action with</typeparam>
        /// <param name="source">The fluent object to perform the actions on</param>
        /// <param name="items">Enumerator returning the items to perform <paramref name="action"/> with. Can be null</param>
        /// <param name="action">Delegate that executes the action. First arg is the last returned fluent object, second arg is the counter and last arg is the item to perform the action with.
        /// The object returned will be the used for the next item. When null is returned the last non-null object is returned</param>
        /// <returns><paramref name="source"/> or the last object returned by <paramref name="action"/></returns>
        public static T ForEach<T, TItem>(this T source, IEnumerable<TItem> items, Func<T, int, TItem, T> action)
        {
            source.ValidateArgument(nameof(source));
            action.ValidateArgument(nameof(action));

            var counter = 0;
            if(items != null)
            {
                foreach (var item in items)
                {
                    var result = action(source, counter, item);
                    if (result == null) return source;
                    source = result;
                    counter++;
                }
            }         

            return source;
        }
        #endregion
    }
}
