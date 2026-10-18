using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Sels.Core.Extensions;

namespace Sels.Core.Progress.Templates
{
    /// <summary>
    /// An implementation of <see cref="IProgress{T}"/> that only reports progress at a given interval. If the progress is updated faster than the interval the progress will not be reported.
    /// </summary>
    /// <typeparam name="T">The type of reported progress</typeparam>
    public abstract class IntervalProgress<T> : IProgress<T>
    {
        // Fields
        /// <summary>
        /// Lock object to use when updating the progress value
        /// </summary>
        protected readonly object _lock = new object();
        /// <summary>
        /// The minimum time between updates. If the progress is updated faster than this interval the progress will not be reported.
        /// </summary>
        protected readonly TimeSpan _interval;
        /// <summary>
        /// The stopwatch used to track the time since the last update.
        /// </summary>
        protected Stopwatch _stopwatch;

        // Properties
        /// <summary>
        /// The last reported value regardless of the interval. This value will always be updated when <see cref="Report(T)"/> is called.
        /// </summary>
        public T LastReported { get; private set; }

        /// <inheritdoc cref="IntervalProgress{T}"/>
        /// <param name="interval"><inheritdoc cref="_interval"/></param>
        public IntervalProgress(TimeSpan interval)
        {
            interval = interval.ValidateArgumentLargerOrEqual(nameof(interval), TimeSpan.Zero);
            _interval = interval;
            _stopwatch = Stopwatch.StartNew();
        }

        /// <inheritdoc/>
        public void Report(T value)
        {
            LastReported = value;

            lock (_lock)
            {
                if (_stopwatch.Elapsed >= _interval)
                {
                    _stopwatch.Restart();
                    OnReport(value);
                }
            }
        }

        /// <summary>
        /// Called when the progress is reported. This method is only called if the time since the last report is greater than or equal to <see cref="_interval"/>.
        /// </summary>
        /// <param name="value">The progress that was reported</param>
        protected abstract void OnReport(T value);
    }
}
