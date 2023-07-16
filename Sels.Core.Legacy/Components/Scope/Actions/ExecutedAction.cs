using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Scope.Actions
{
    /// <summary>
    /// Sets a bool to false when creating this action and sets it to true when it is disposed.
    /// </summary>
    public class ExecutedAction : ScopedAction
    {
        /// <inheritdoc cref="ExecutedAction"/>
        /// <param name="setter">The action that will get called with the boolean to set</param>
        public ExecutedAction(Action<bool> setter) : base(() => setter.ValidateArgument(nameof(setter))(false), () => setter(true))
        {

        }
    }
}
