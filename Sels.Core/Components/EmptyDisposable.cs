using System;

namespace Sels.Core.Components
{
    /// <summary>
    /// A disposable object that does nothing when disposed. Useful for performance optimization 
    /// when a method needs to return an IDisposable but no actual cleanup is needed.
    /// </summary>
    public sealed class EmptyDisposable : IDisposable
    {
        /// <summary>
        /// Singleton instance to avoid creating unnecessary objects.
        /// </summary>
        public static readonly EmptyDisposable Instance = new EmptyDisposable();

        /// <summary>
        /// Private constructor to enforce singleton pattern.
        /// </summary>
        private EmptyDisposable()
        {
        }

        /// <summary>
        /// Does nothing when disposed.
        /// </summary>
        public void Dispose()
        {
            // Intentionally empty
        }
    }
}