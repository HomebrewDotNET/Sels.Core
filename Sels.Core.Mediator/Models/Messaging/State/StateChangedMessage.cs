using Sels.Core.Extensions.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Mediator.Models.Messages.State
{
    /// <summary>
    /// Message containing objects who had a state change.
    /// </summary>
    /// <typeparam name="T">Type of the objects that changed state</typeparam>
    public class StateChangedMessage<T>
    {
        // Properties
        /// <inheritdoc cref="ObjectLifecycle"/>
        public ObjectLifecycle Lifecycle { get; }
        /// <summary>
        /// Optional object containing a reference to the object that triggered the state change.
        /// </summary>
        public object? Initiator { get; }
        /// <summary>
        /// Array of the objects had their state changed.
        /// </summary>
        public T[] Objects { get; }
        /// <summary>
        /// Optional reason why the objects were changed.
        /// </summary>
        public string? Reason { get; set; }

        /// <inheritdoc cref="StateChangedMessage{T}"/>
        /// <param name="lifecycle"><see cref="Lifecycle"/></param>
        /// <param name="objects"><see cref="Objects"/></param>
        /// <param name="initiator"><see cref="Initiator"/></param>
        /// <param name="reason"><see cref="Reason"/></param>
        public StateChangedMessage(ObjectLifecycle lifecycle, IEnumerable<T> objects, object? initiator = null, string? reason = null)
        {
            Lifecycle = lifecycle;
            Objects = objects.ValidateArgumentNotNullOrEmpty(nameof(objects)).ToArray();
            Initiator = initiator;
            Reason = reason;
        }

        /// <inheritdoc cref="StateChangedMessage{T}"/>
        /// <param name="lifecycle"><see cref="Lifecycle"/></param>
        /// <param name="changed"><see cref="Objects"/></param>
        /// <param name="initiator"><see cref="Initiator"/></param>
        /// <param name="reason"><see cref="Reason"/></param>
        public StateChangedMessage(ObjectLifecycle lifecycle, T changed, object? initiator = null, string? reason = null) : this(lifecycle, changed.ValidateArgument(nameof(changed)).AsArray(), initiator, reason)
        {
        }
    }

    /// <summary>
    /// Where in the lifecycle of an object the state change was triggered.
    /// </summary>
    public enum ObjectLifecycle
    {
        /// <summary>
        /// Object is new.
        /// </summary>
        Created,
        /// <summary>
        /// Existing object was updated.
        /// </summary>
        Updated,
        /// <summary>
        /// Existing object was deleted.
        /// </summary>
        Delete
    }
}
