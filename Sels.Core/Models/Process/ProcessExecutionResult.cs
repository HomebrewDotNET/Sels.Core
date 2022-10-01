using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Process
{
    /// <summary>
    /// Contains the result from executing a process.
    /// </summary>
    public class ProcessExecutionResult
    {
        /// <summary>
        /// The exit code returned by the process.
        /// </summary>
        public int ExitCode { get; }
        /// <summary>
        /// All the lines outputted from the standard output.
        /// </summary>
        public string[] StandardOutputLines { get; set; }
        /// <summary>
        /// The standard output of the process.
        /// </summary>
        public string StandardOutput => StandardOutputLines.JoinStringNewLine();
        /// <summary>
        /// All the lines outputted from the error output.
        /// </summary>
        public string[] ErrorOutputLines { get; set; }
        /// <summary>
        /// The error output of the process.
        /// </summary>
        public string ErrorOutput => ErrorOutputLines.JoinStringNewLine();
        /// <summary>
        /// When the process was started.
        /// </summary>
        public DateTime StartedTime { get; }
        /// <summary>
        /// When the process exited.
        /// </summary>
        public DateTime ExitTime { get; }
        /// <summary>
        /// How long the process ran for.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <inheritdoc cref="ProcessExecutionResult"/>
        /// <param name="exitCode"><inheritdoc cref="ExitCode"/></param>
        /// <param name="standardOutputLines"><inheritdoc cref="StandardOutputLines"/></param>
        /// <param name="errorOutputLines"><inheritdoc cref="ErrorOutputLines"/></param>
        /// <param name="startedTime"><inheritdoc cref="StartedTime"/></param>
        /// <param name="exitTime"><inheritdoc cref="ExitTime"/></param>
        /// <param name="duration"><inheritdoc cref="Duration"/></param>
        public ProcessExecutionResult(int exitCode, IEnumerable<string> standardOutputLines, IEnumerable<string> errorOutputLines, DateTime startedTime, DateTime exitTime, TimeSpan duration)
        {
            ExitCode = exitCode;
            StandardOutputLines = standardOutputLines.ValidateArgument(nameof(standardOutputLines)).ToArray();
            ErrorOutputLines = errorOutputLines.ValidateArgument(nameof(errorOutputLines)).ToArray();
            StartedTime = startedTime;
            ExitTime = exitTime;
            Duration = duration;
        }
    }
}
