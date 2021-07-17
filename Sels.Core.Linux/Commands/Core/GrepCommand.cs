using Sels.Core.Extensions;
using Sels.Core.Linux.Components.LinuxCommand;
using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using Sels.Core.Linux.Templates.LinuxCommand;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Commands.Core
{
    /// <summary>
    /// Used to filter results from other commands / files matching the pattern
    /// </summary>
    public class GrepCommand : BaseLinuxCommand<string, LinuxCommandResult<string[], string>>
    {
        // Properties
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

        public GrepCommand(string pattern) : this()
        {
            Pattern = pattern;
        }

        public GrepCommand() : base(LinuxConstants.Commands.Grep)
        {

        }

        public override LinuxCommandResult<string[], string> CreateResult(bool wasSuccesful, int exitCode, string output, string error)
        {
            if (wasSuccesful)
            {
                return new LinuxCommandResult<string[], string>(output.SplitStringOnNewLine(), exitCode);
            }
            else
            {
                return new LinuxCommandResult<string[], string>(error, exitCode);
            }
        }
    }
}
