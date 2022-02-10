using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Scope.Actions
{
    /// <summary>
    /// Sets a bool to true when creating this action and sets it to false when it is disposed.
    /// </summary>
    public class InProcessAction : ScopedAction
    {
        /// <summary>
        /// Sets a bool to true when creating this action and sets it to false when it is disposed.
        /// </summary>
        /// <param name="setter">The action that will get called with the boolean to set</param>
        public InProcessAction(Action<bool> setter) : base(() => setter.ValidateArgument(nameof(setter))(true), () => setter(false))
        {

        }
    }
}
