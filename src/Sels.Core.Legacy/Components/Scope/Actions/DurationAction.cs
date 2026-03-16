using Sels.Core.Extensions;
using Sels.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Sels.Core.Scope.Actions
{
    /// <summary>
    /// Captures the duration of the scope.
    /// </summary>
    public class DurationAction : IDisposable
    {
        // Fields
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly Ref<TimeSpan> _ref;

        /// <inheritdoc cref="DurationAction"/>
        /// <param name="reference">The reference to output the duraction to</param>
        public DurationAction(Ref<TimeSpan> reference)
        {
            _ref = reference.ValidateArgument(nameof(reference));
            _stopwatch.Start();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _stopwatch.Stop();
            _ref.Value = _stopwatch.Elapsed;
        }
    }
}
