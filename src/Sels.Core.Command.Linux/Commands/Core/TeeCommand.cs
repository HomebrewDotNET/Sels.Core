using Sels.Core.Command.Linux.Templates;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Command.Linux.Attributes;

namespace Sels.Core.Command.Linux.Commands.Core
{
    /// <summary>
    /// Captures input and streams it to 1 or more files and the standard output 
    /// </summary>
    public class TeeCommand : BaseLinuxCommand<string>
    {
        // Properties
        /// <summary>
        /// The filenames of files to also pipe the output to.
        /// </summary>
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
        /// <inheritdoc cref="TeeCommand"/>
        /// <param name="file">The file to pipe the output to</param>
        public TeeCommand(string file) : this(file.AsList())
        {

        }
        /// <inheritdoc cref="TeeCommand"/>
        /// <param name="files">The files to pipe the output to</param>
        public TeeCommand(IEnumerable<string> files) : this()
        {
            Files = new List<string>(files);
        }
        /// <inheritdoc cref="TeeCommand"/>
        public TeeCommand() : base(LinuxCommandConstants.Commands.Tee)
        {

        }
    }
}
