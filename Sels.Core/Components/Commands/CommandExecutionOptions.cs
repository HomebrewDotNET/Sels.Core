using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Sels.Core.Contracts.Commands;
using Sels.Core.Extensions.Conversion;

namespace Sels.Core.Components.Commands
{
    /// <summary>
    /// Provides extra options when executing a <see cref="ICommand"/>.
    /// </summary>
    public class CommandExecutionOptions
    {
        public CommandExecutionOptions(IEnumerable<ILogger> loggers = null)
        {
            if(loggers != null)
            {
                Loggers.AddRange(loggers);
            }
        }

        public CommandExecutionOptions(ILogger logger = null) : this(logger.AsArrayOrDefault())
        {

        }

        // Properties
        #region Execution
        /// <summary>
        /// Token for cancelling the execution of long running <see cref="ICommand"/>.
        /// </summary>
        public CancellationToken Token { get; set; }
        #endregion

        #region Result
        /// <summary>
        /// Command will succeed when the exit code returned from executing a <see cref="ICommand"/> is equal to this value. If left null the default success exit code of the <see cref="ICommand"/> will be used. 
        /// </summary>
        public int? SuccessExitCode { get; set; }
        /// <summary>
        /// If set to true the <see cref="ICommand"/> execution will fail regardless of the exit code returned when the error output has value.
        /// </summary>
        public bool FailOnErrorOutput { get; set; } = false;
        #endregion

        #region Logging
        /// <summary>
        /// Allows <see cref="ICommand"/> to log.
        /// </summary>
        public List<ILogger> Loggers { get; } = new List<ILogger>();
        #endregion
    }
}
