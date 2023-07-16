using Sels.Core.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Scope
{
    /// <summary>
    /// Executes an async action when this object gets created and an async action when this object gets disposed.
    /// </summary>
    public class AsyncScopedAction : IAsyncDisposable
    {
        // Fields
        private readonly AsyncAction<CancellationToken> _startAction;
        private readonly AsyncAction<CancellationToken> _endAction;
        private CancellationToken _token;

        /// <inheritdoc cref="AsyncScopedAction"/>
        /// <param name="startAction">The action that is to be executed by <see cref="StartAsync(CancellationToken)"/></param>
        /// <param name="endAction">The action that is to be executed when disposing the scoped action</param>
        public AsyncScopedAction(AsyncAction<CancellationToken> startAction, AsyncAction<CancellationToken> endAction)
        {
            _startAction = startAction.ValidateArgument(nameof(startAction));
            _endAction = endAction.ValidateArgument(nameof(endAction));
        }

        /// <inheritdoc cref="AsyncScopedAction"/>
        /// <param name="startAction">The action that is to be executed by <see cref="StartAsync(CancellationToken)"/></param>
        /// <param name="endAction">The action that is to be executed when disposing the scoped action</param>
        public AsyncScopedAction(AsyncAction startAction, AsyncAction<CancellationToken> endAction) : this(async x => await startAction(), endAction)
        {
            startAction.ValidateArgument(nameof(startAction));
        }

        /// <inheritdoc cref="AsyncScopedAction"/>
        /// <param name="startAction">The action that is to be executed by <see cref="StartAsync(CancellationToken)"/></param>
        /// <param name="endAction">The action that is to be executed when disposing the scoped action</param>
        public AsyncScopedAction(AsyncAction<CancellationToken> startAction, AsyncAction endAction) : this(startAction, async x => await endAction())
        {
            endAction.ValidateArgument(nameof(endAction));
        }

        /// <inheritdoc cref="AsyncScopedAction"/>
        /// <param name="startAction">The action that is to be executed by <see cref="StartAsync(CancellationToken)"/></param>
        /// <param name="endAction">The action that is to be executed when disposing the scoped action</param>
        public AsyncScopedAction(AsyncAction startAction, AsyncAction endAction) : this(async x => await startAction(), async x => await endAction())
        {
            startAction.ValidateArgument(nameof(startAction));
            endAction.ValidateArgument(nameof(endAction));
        }

        /// <summary>
        /// Executes the provided start action.
        /// </summary>
        /// <param name="token">Optional token provided to the start/end actions</param>
        /// <returns>Current scoped action so it can be disposed</returns>
        public async Task<IAsyncDisposable> StartAsync(CancellationToken token = default)
        {
            _token = token;
            await _startAction(_token);
            return this;
        }

        /// <summary>
        /// Triggers to end action.
        /// </summary>
        /// <returns>Task to await the result</returns>
        public async ValueTask DisposeAsync()
        {
            await _endAction(_token);
        }
    }
}
