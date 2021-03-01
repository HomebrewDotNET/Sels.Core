using Sels.Core.Extensions;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static Sels.Core.Components.RecurrentAction.RecurrentActionDelegates;

namespace Sels.Core.Components.RecurrentAction
{
    internal class RepeatingMethod<T> : RecurrentMethod<T>
    {
        protected override string ClassName { get;} = "Sels.Core.Components.RecurrentAction.RepeatingMethod<T>.";

        internal RepeatingMethod(T identifier, int downtime, Action<T, CancellationToken> entryMethod, RecurrentActionExceptionHandler<T> exceptionHandler) : base(identifier, downtime, entryMethod, exceptionHandler)
        {

        }

        internal RepeatingMethod(T identifier, int downtime, Action<T, CancellationToken> entryMethod, RecurrentActionExceptionHandler<T> exceptionHandler, RecurrentActionElapsedHandler<T> elapsedHandler) : base(identifier, downtime, entryMethod, exceptionHandler, elapsedHandler)
        {
        }

        protected override void ElapsedHandler(object sender, ElapsedEventArgs args)
        {
            string MethodName = ClassName + "ElapsedHandler";

            try
            {
                _timer.Stop();

                if (!_cancelationSource.IsCancellationRequested)
                {
                    if (sender.HasValue() && args.HasValue() && _elapsedHandler.HasValue())
                    {
                        _elapsedHandler(_identifier, sender, args);
                    }

                    _isRunning = true;
                    _entryMethod(_identifier, _cancelationSource.Token);                  
                }

                _isRunning = false;
            }
            catch (Exception ex)
            {
                HandleException(MethodName, ex);
            }
            finally
            {
                lock (_threadLock)
                {
                    if (!_cancelationSource.IsCancellationRequested)
                    {
                        _timer.Start();
                    }
                }
            }
        }
    }
}
