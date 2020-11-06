using Sels.Core.Components.RecurringAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using static Sels.Core.Components.RecurrentAction.RecurrentActionDelegates;

namespace Sels.Core.Components.RecurrentAction
{
    public class RepeatingActionManager<T> : RecurrentActionManager<T>
    {
        public RepeatingActionManager()
        {

        }

        public override void AddRecurrentAction(T identifier, int downtime, Action<T, CancellationToken> entryMethod, RecurrentActionExceptionHandler<T> exceptionHandler, RecurrentActionElapsedHandler<T> elapsedHandler) {
            if (_recurrentMethods.Any(x => x._identifier.Equals(identifier))){
                throw new IdentifierNotUniqueException<T>(identifier);
            }
            
            var repeatingAction = new RepeatingMethod<T>(identifier, downtime, entryMethod, exceptionHandler, elapsedHandler);

            _recurrentMethods.Add(repeatingAction);
        }

    }
}
