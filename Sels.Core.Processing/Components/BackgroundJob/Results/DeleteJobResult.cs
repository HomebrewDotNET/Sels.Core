using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Signals the worker node that it can delete the job this result was returned from.
    /// </summary>
    public class DeleteJobResult : IBackgroundJobResult
    {
        /// <inheritdoc/>
        public object ActualResult { get; init; }
        /// <inheritdoc/>
        public string Reason { get; init; }
    }
}
