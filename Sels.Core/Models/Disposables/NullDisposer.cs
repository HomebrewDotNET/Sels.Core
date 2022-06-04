using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Models.Disposables
{
    /// <summary>
    /// Class that implements <see cref="IDisposable"/> but does nothing in the dispose.
    /// </summary>
    public class NullDisposer : IDisposable
    {
        private NullDisposer()
        {

        }

        /// <inheritdoc/>
        public void Dispose()
        {

        }

        // Statics
        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static NullDisposer Instance { get; } = new NullDisposer();
    }
}
