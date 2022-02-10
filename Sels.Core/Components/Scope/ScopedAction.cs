using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Scope
{
    /// <summary>
    /// Executed an action when this object gets created and an action when this object gets disposed.
    /// </summary>
    public class ScopedAction : IDisposable
    {
        // Fields
        private readonly Action _stopAction;

        /// <summary>
        /// Creates a new action and executed <paramref name="startAction"/> when this action is constructed.
        /// </summary>
        /// <param name="startAction">Action to execute when object is constructed</param>
        /// <param name="stopAction">Action to execute when object is disposed</param>
        public ScopedAction(Action startAction, Action stopAction)
        {
            startAction.ValidateArgument(nameof(startAction));
            _stopAction = stopAction.ValidateArgument(nameof(stopAction));

            startAction();
        }

        /// <summary>
        /// Triggers the stop action.
        /// </summary>
        public void Dispose()
        {
            _stopAction();
        }
    }
}
