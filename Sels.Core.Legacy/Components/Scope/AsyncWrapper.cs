using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Components.Scope
{
    /// <summary>
    /// Class that wraps an object and executes actions when starting and disposing the object.
    /// </summary>
    /// <typeparam name="T">TThe type of the object to wrap</typeparam>
    public class AsyncWrapper<T> : AsyncScopedAction
    {
        // Properties
        /// <summary>
        /// The wrapped object.
        /// </summary>
        public T Value { get; }

        /// <inheritdoc cref="AsyncWrapper{T}"/>
        /// <param name="value"><inheritdoc cref="Value"/></param>
        /// <param name="startAction">Optional start action</param>
        /// <param name="stopAction">Optional dispose action</param>
        public AsyncWrapper(T value, AsyncAction<T, CancellationToken> startAction = null, AsyncAction<T, CancellationToken> stopAction = null) : base(async x => 
        {
            if(startAction != null)
            {
                await startAction.Invoke(value, x);
            }
        }, async x =>
        {
            if (stopAction != null)
            {
                await stopAction.Invoke(value, x);
            }
        })
        {
            Value = value.ValidateArgument(nameof(value));
        }
    }
}
