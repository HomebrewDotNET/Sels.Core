using CommandLine;
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
        /// Log more about what the runner is doing.
        /// </summary>
        [Option('v')]
        public bool Verbose { get; set; }
        /// <summary>
        /// Logs more for debugging.
        /// </summary>
        [Option('d')]
        public bool Debug { get; set; }

        /// <summary>
        /// Takes settings from <paramref name="other"/> if they aren't set in the current builder.
        /// </summary>
        /// <param name="other"></param>
        public void MergeFrom(CliArguments other)
        {
            other.ValidateArgument(nameof(other));

            Provider ??= other.Provider;
            Type ??= other.Type;
            if (!Verbose) Verbose = other.Verbose;
            if (!Debug) Debug = other.Debug;
        }
    }
}
