using Sels.Core.Extensions;
using Sels.Core.Extensions.Execution;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Sels.Core.Components.Display.Progress
{
    public class ProgressSpinner : IDisposable
    {
        // Statics
        private readonly static string[] _spinParts = new string[] { "|", "/", "-", "\\"};

        // Fields
        private readonly Timer _timer;
        private int _spinCounter;
        private readonly object _threadLock = new object();

        // Properties
        private string Message { get; set; }
        private Action DisposeAction { get; }

        public ProgressSpinner(string message, int interval, Action<string> valueUpdater, Action<string> initialValueUpdater = null, Action disposeAction = null)
        {
            message.ValidateVariable(nameof(message));
            interval.ValidateVariable(x => x > 1, x => $"{nameof(interval)} must be greater than 1. Was <{x}>");
            valueUpdater.ValidateVariable(nameof(valueUpdater));

            Message = message;
            DisposeAction = disposeAction;
            _timer = new Timer()
            {
                AutoReset = true,
                Interval = interval,
            };
            _timer.Elapsed += (x, y) => Spin(valueUpdater);

            initialValueUpdater.IfHasValue(() => Spin(initialValueUpdater));

            _timer.Start();
        }

        private void Spin(Action<string> valueUpdater)
        {
            var spinPart = _spinParts[_spinCounter % _spinParts.Length];

            lock (_threadLock)
            {
                valueUpdater($"{Message} {spinPart}");
            }

            _spinCounter++;
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();

            DisposeAction.IfHasValue(() => DisposeAction());
        }
    }
}
