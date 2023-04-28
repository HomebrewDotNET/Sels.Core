using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Signals the worker node that it should fail the current job without retrying it automatically.
    /// </summary>
    public class FailJobResult : IBackgroundJobResult
    {
        /// <inheritdoc/>
        public object ActualResult { get; init; }
        /// <inheritdoc/>
        public string Reason { get; init; }
    }
}
