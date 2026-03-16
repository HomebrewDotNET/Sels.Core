using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Dispose
{
    /// <summary>
    /// Provides a mechanism for releasing unmanaged resources asynchronously and exposes the current dispose state.
    /// </summary>
    public interface IAsyncExposedDisposable : IDisposableState, IAsyncDisposable
    {
    }
}
