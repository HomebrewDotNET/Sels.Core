using Microsoft.Extensions.Logging;
using Sels.Core.Command.Linux.Templates;
using Sels.Core.Command.Linux.Attributes;
using Sels.Core.Command.Linux.Contracts;

namespace Sels.Core.Command.Linux.Commands.Core
{
    /// <summary>
    /// Used to filter results from other commands / files matching the pattern
    /// </summary>
    public class GrepCommand : BaseLinuxCommand<string, ILinuxCommandResult<string[], string>>
    {
        // Properties
        /// <summary>
        /// The pattern used to filter.
        /// </summary>
        [TextArgument(order: 1, required: true)]
        public string Pattern { get; set; }

        [TextListArgument(order: 2)]
        public string[] Targets { get; set; }

        [FlagArgument("-w" ,  order: 0)]
        public bool OnlyWholeWord { get; set; }
        [FlagArgument("-i", order: 0)]
        public bool IgnoreCase { get; set; }

        [FlagArgument("-v", order: 0)]
        public bool IsInvertSearch { get; set; }

        [FlagArgument("-r", order: 0)]
        public bool IsRecursive { get; set; }

        [FlagArgument("-x", order: 0)]
        public bool OnlyExactMatch { get; set; }

        [FlagArgument("-l", order: 0)]
        public bool ListMatchingFileNames { get; set; }

        [FlagArgument("-c", order: 0)]
        public bool CountMatches { get; set; }

        [TextArgument(prefix: "-A", order: 0)]
        public int? LinePrefixCount { get; set; }

        [TextArgument(prefix: "-B", order: 0)]
        public int? LineSuffixCount { get; set; }

        [TextArgument(prefix: "-C", order: 0)]
        public int? LinePrefixAndSuffixCount { get; set; }

        [TextArgument(prefix: "-m", format: TextArgument.PrefixFormat + TextArgument.ValueFormat, order: 0)]
        public int? MaxResults { get; set; }
        /// <inheritdoc cref="GrepCommand"/>
        /// <param name="pattern">The pattern for the grep command</param>
        public GrepCommand(string pattern) : this()
        {
            Pattern = pattern;
        }
        /// <inheritdoc cref="GrepCommand"/>
        public GrepCommand() : base(LinuxCommandConstants.Commands.Grep)
        {

        }

        /// <inheritdoc/>
        public override ILinuxCommandResult<string[], string> CreateResult(bool wasSuccesful, int exitCode, string? output, string? error, IEnumerable<ILogger>? loggers = null)
        {
            if (wasSuccesful)
            {
                return new LinuxCommandResult<string[], string>(output.SplitOnNewLine(), exitCode);
            }
            else
            {
                return new LinuxCommandResult<string[], string>(error, exitCode);
            }
        }
    }
}
