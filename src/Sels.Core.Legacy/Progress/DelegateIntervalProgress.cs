using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Progress
{
    /// <summary>
    /// An implementation of <see cref="Templates.IntervalProgress{T}"/> that uses a delegate to report progress.
    /// </summary>
    /// <typeparam name="T">The type of reported progress</typeparam>
    public class DelegateIntervalProgress<T> : Templates.IntervalProgress<T>
    {
        // Fields
        private readonly Action<T> _reportAction;
        /// <inheritdoc cref="DelegateIntervalProgress{T}"/>
        /// <param name="interval"><inheritdoc cref="Templates.IntervalProgress{T}._interval"/></param>
        /// <param name="reportAction">Delegate to call when the progress is reported</param>
        public DelegateIntervalProgress(TimeSpan interval, Action<T> reportAction) : base(interval)
        {
            _reportAction = reportAction.ValidateArgument(nameof(reportAction));
        }
        /// <inheritdoc/>
        protected override void OnReport(T value)
        {
            _reportAction(value);
        }
    }
}
