using CommandLine;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.IntegrationTester
{
    /// <summary>
    /// The cli arguments for this tool.
    /// </summary>
    public class CliArguments
    {
        /// <summary>
        /// What providers to test.
        /// </summary>
        [Option('p', Required = false)]
        public TestProvider? Provider { get; set; }
        /// <summary>
        /// What type of test to execute.
        /// </summary>
        [Option('t', Required = false)]
        public TestType? Type { get; set; }
        /// <summary>
        /// Optional directories where config files can be loaded from. When set it doesn't check the app directory.
        /// </summary>
        [Option('c', Required = false)]
        public IEnumerable<string> ConfigDirectories { get; set; }
        /// <summary>
        /// Selects the log level for the tracing. Set to <see cref="LogLevel.None"/> to disable logging.
        /// </summary>
        [Option('v')]
        public LogLevel LogLevel { get; set; } = LogLevel.None;
        /// <summary>
        /// If only logging for the tester should be enabled.
        /// </summary>
        [Option('l')]
        public bool OnlyTesterLogging { get; set; }

        /// <summary>
        /// Takes settings from <paramref name="other"/> if they aren't set in the current builder.
        /// </summary>
        /// <param name="other"></param>
        public void MergeFrom(CliArguments other)
        {
            other.ValidateArgument(nameof(other));

            Provider ??= other.Provider;
            Type ??= other.Type;
            if (LogLevel == LogLevel.None) LogLevel = other.LogLevel;
            if (!OnlyTesterLogging) OnlyTesterLogging = other.OnlyTesterLogging;
        }
    }
}
