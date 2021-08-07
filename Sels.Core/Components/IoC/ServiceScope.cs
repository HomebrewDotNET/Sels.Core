using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.IoC
{
    /// <summary>
    /// Scope that dictates when a new instance should be created when the service gets resolved.
    /// </summary>
    public enum ServiceScope
    {
        /// <summary>
        /// A new instance is always created.
        /// </summary>
        Transient,
        /// <summary>
        /// The same instance is resolved within the same scope.
        /// </summary>
        Scoped,
        /// <summary>
        /// Only 1 instance is created.
        /// </summary>
        Singleton
    }
}
