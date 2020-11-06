using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using static Sels.Core.Components.RecurrentAction.RecurrentActionDelegates;

namespace Sels.Core.Components.RecurrentAction
{
    public interface IRecurrentAction<T>
    {
        T Identifier { get; }
        int DownTime { get; }
        Action<T, CancellationToken> EntryMethod { get; }
        RecurrentActionExceptionHandler<T> ExceptionHandler { get; }
        RecurrentActionElapsedHandler<T> ElaspedHandler { get; }
    }
}
