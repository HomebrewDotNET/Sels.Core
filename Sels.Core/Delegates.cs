using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core
{
    /// <summary>
    /// Contains delegate definitions.
    /// </summary>
    public static class Delegates
    {
        /// <summary>
        /// Encapsulates a method that compares to objects of type <typeparamref name="T"/> to see if they are equal, matching, ...
        /// </summary>
        /// <typeparam name="T">Type of objects to compare</typeparam>
        /// <param name="arg1">Object to compare</param>
        /// <param name="arg2">Object to compare</param>
        /// <returns>Boolean indicating if <param name="arg1"/> is equal, matching, ... to <param name="arg2"/></returns>
        public delegate bool Comparator<T>(T arg1, T arg2);
    }
}
