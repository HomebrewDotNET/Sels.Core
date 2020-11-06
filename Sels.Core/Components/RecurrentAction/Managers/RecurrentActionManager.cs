using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using static Sels.Core.Components.RecurrentAction.RecurrentActionDelegates;

namespace Sels.Core.Components.RecurrentAction
{
    public abstract class RecurrentActionManager<T>
    {
        // State
        internal protected readonly List<RecurrentMethod<T>> _recurrentMethods;

        public RecurrentActionManager()
        {
            _recurrentMethods = new List<RecurrentMethod<T>>();
        }

        public void StartAll()
        {
            foreach (var method in _recurrentMethods)
            {
                method.Start();
            }
        }

        public void StopAll()
        {
            foreach(var method in _recurrentMethods)
            {
                method.Stop();
            }
        }

        public void WaitAll()
        {
            foreach(var method in _recurrentMethods)
            {
                method.Wait();
            }
        }

        public void StopAndWaitAll()
        {
            StopAll();
            WaitAll();
        }

        public abstract void AddRecurrentAction(T identifier, int downtime, Action<T, CancellationToken> entryMethod, RecurrentActionExceptionHandler<T> exceptionHandler, RecurrentActionElapsedHandler<T> elapsedHandler);

        public void AddRecurrentAction(T identifier, int downtime, Action<T, CancellationToken> entryMethod, RecurrentActionExceptionHandler<T> exceptionHandler)
        {
            AddRecurrentAction(identifier, downtime, entryMethod, exceptionHandler, null);
        }

        public void AddRecurrentAction(IRecurrentAction<T> recurrentAction) {
            AddRecurrentAction(recurrentAction.Identifier, recurrentAction.DownTime, recurrentAction.EntryMethod, recurrentAction.ExceptionHandler, recurrentAction.ElaspedHandler);
        }
    }
}
