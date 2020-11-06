using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Sels.Core.Components.RecurrentAction
{
    public static class RecurrentActionDelegates
    {
        public delegate void RecurrentActionExceptionHandler<T>(T identifier, string sourceMethod, Exception exception);
        public delegate void RecurrentActionElapsedHandler<T>(T identifier, object source, ElapsedEventArgs args);
    }
}
