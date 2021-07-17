using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using Sels.Core.Linux.Templates.LinuxCommand;
using Sels.Core.Extensions.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Linux.Commands.Core
{
    /// <summary>
    /// Captures input and streams it to 1 or more files and the standard output 
    /// </summary>
    public class TeeCommand : BaseLinuxCommand<string>
    {
        // Properties
        [TextListArgument( order: 2, parsingOption: TextParsingOptions.DoubleQuotes)]
        public List<string> Files { get; set; }
        /// <summary>
        /// Append output to end of file instead of overwriting.
        /// </summary>
        [FlagArgument("-a", 1)]
        public bool Append { get; set; }
        /// <summary>
        /// Ignore interrupt signals.
        /// </summary>
        [FlagArgument("-i", 1)]
        public bool IgnoreInterrupts { get; set; }

        public TeeCommand(string file) : this(file.AsList())
        {

        }

        public TeeCommand(IEnumerable<string> files) : this()
        {
            Files = new List<string>(files);
        }

        public TeeCommand() : base(LinuxConstants.Commands.Tee)
        {

        }
    }
}
