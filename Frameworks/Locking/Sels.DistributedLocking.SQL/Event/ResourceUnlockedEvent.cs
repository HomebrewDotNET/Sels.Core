using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.DistributedLocking.SQL.Event
{
    /// <summary>
    /// Raised when a resource is unlocked.
    /// </summary>
    internal class ResourceUnlockedEvent
    {
        // Properties
        /// <summary>
        /// The resource that was unlocked.
        /// </summary>
        public string Resource { get; }

        /// <inheritdoc cref="ResourceUnlockedEvent"/>
        /// <param name="resource"><inheritdoc cref="Resource"/></param>
        public ResourceUnlockedEvent(string resource)
        {
            Resource = resource.ValidateArgumentNotNullOrWhitespace(nameof(resource));
        }
    }
}
