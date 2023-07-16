using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Dispose
{
    /// <summary>
    /// Exposes the dispose state.
    /// </summary>
    public interface IDisposableState
    {
        /// <summary>
        /// Indicates the current dispose state. Null means not disposed, false means disposing and true means disposed.
        /// </summary>
        bool? IsDisposed { get; }
    }
}
