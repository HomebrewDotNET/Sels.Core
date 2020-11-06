using Sels.Core.Extensions;
using Sels.Core.Extensions.General.Generic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static Sels.Core.Components.RecurrentAction.RecurrentActionDelegates;
using Timer = System.Timers.Timer;

namespace Sels.Core.Components.RecurrentAction
{
    public abstract class RecurrentMethod<T>
    {
        // Constants
        protected abstract string ClassName { get; }

        // Fields
        internal readonly T _identifier;
        protected readonly int _downtime;
        protected readonly Action<T, CancellationToken> _entryMethod;
        protected readonly RecurrentActionExceptionHandler<T> _exceptionHandler;
        protected readonly RecurrentActionElapsedHandler<T> _elapsedHandler;
        protected readonly object _threadLock = new object();

        // State
        protected Timer _timer;
        protected bool _isRunning = false;
        protected CancellationTokenSource _cancelationSource = new CancellationTokenSource();

        internal RecurrentMethod(T identifier, int downtime, Action<T, CancellationToken> entryMethod, RecurrentActionExceptionHandler<T> exceptionHandler)
        {
            _identifier = identifier;
            _downtime = downtime;
            _entryMethod = entryMethod;
            _exceptionHandler = exceptionHandler;

            Initialize();
            Validate();
        }

        internal RecurrentMethod(T identifier, int downtime, Action<T, CancellationToken> entryMethod, RecurrentActionExceptionHandler<T> exceptionHandler, RecurrentActionElapsedHandler<T> elapsedHandler)
        {
            _identifier = identifier;
            _downtime = downtime;
            _entryMethod = entryMethod;
            _exceptionHandler = exceptionHandler;
            _elapsedHandler = elapsedHandler;

            Initialize();
            Validate();
        }

        internal void Start()
        {
            string MethodName = ClassName + "Start";

            try
            {
                lock (_threadLock)
                {
                    if (!_isRunning)
                    {
                        _cancelationSource = new CancellationTokenSource();
                        Task.Run(() => Execute());
                    }
                }            
            }
            catch (Exception ex)
            {
                HandleException(MethodName, ex);
            }
        }

        internal void Stop()
        {
            string MethodName = ClassName + "Stop";

            try
            {
                lock (_threadLock)
                {
                    _cancelationSource.Cancel();
                    _timer.Stop();
                }                   
            }
            catch (Exception ex)
            {
                HandleException(MethodName, ex);
            }
        }

        internal void Wait()
        {
            string MethodName = ClassName + "Wait";

            try
            {
                while (_isRunning)
                {
                    Thread.Sleep(250);
                }
            }
            catch (Exception ex)
            {
                HandleException(MethodName, ex);
            }
        }

        protected void Execute()
        {
            string MethodName = ClassName + "Execute";

            try
            {
                ElapsedHandler(this, null);
            }
            catch (Exception ex)
            {
                HandleException(MethodName, ex);
            }
        }

        protected void Initialize()
        {
            _timer = new Timer(_downtime);
            _timer.AutoReset = true;
            _timer.Elapsed += ElapsedHandler;
        }

        protected void Validate()
        {
            if (!_identifier.HasValue()) throw new ArgumentNullException(nameof(_identifier));
            if (!_downtime.HasValue()) throw new ArgumentNullException(nameof(_downtime));
            if (!_entryMethod.HasValue()) throw new ArgumentNullException(nameof(_entryMethod));
            if (!_exceptionHandler.HasValue()) throw new ArgumentNullException(nameof(_exceptionHandler));
        }

        protected abstract void ElapsedHandler(object sender, ElapsedEventArgs args);

        protected void HandleException(string sourceMethod, Exception exception)
        {
            if (_exceptionHandler.HasValue())
            {
                _exceptionHandler(_identifier, sourceMethod, exception);
            }
        }
    }
}
