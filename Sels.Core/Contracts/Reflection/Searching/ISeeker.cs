using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sels.Core.Contracts.Reflection.Searching
{
    /// <summary>
    /// Searches through an objects hierarchy and returns all objects matching the defined conditions.
    /// </summary>
    /// <typeparam name="T">The object type to search for</typeparam>
    public interface ISeeker<out T>
    {
        /// <summary>
        /// Only objects that pass all <paramref name="conditions"/> will be returned.
        /// </summary>
        /// <param name="conditions">The conditions that objects must pass before they are returned by this seeker</param>
        /// <returns>Current instance for method chaining</returns>
        ISeeker<T> ReturnWhen(params Predicate<T>[] conditions);
        /// <summary>
        /// Searches all properties on <paramref name="objectToSearch"/> and properties on the objects containing in the properties on <paramref name="objectToSearch"/> for all instances of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="objectToSearch">The object to search</param>
        /// <param name="additionalObjectsToSearch">Optional additional objects to search</param>
        /// <returns>An enumerator that returns all instances of <typeparamref name="T"/> in <paramref name="objectToSearch"/></returns>
        IEnumerable<T> SearchAll(object objectToSearch, params object[] additionalObjectsToSearch);
    }
}
