using Microsoft.Extensions.Logging;
using Sels.Core.Command.Linux.Attributes;

namespace Sels.Core.Command.Linux.Templates.Commands.FileSystem
{
    /// <summary>
    /// Linux command for getting information about a file system member.
    /// </summary>
    public abstract class DfCommand<TCommandResult> : BaseLinuxCommand<string, TCommandResult>
    {
        // Properties
        /// <summary>
        /// Only list local file systems.
        /// </summary>
        [FlagArgument("-l", order: 1)]
        public bool OnlyLocal { get; set; }
        /// <summary>
        /// Sync the file systems before getting the info
        /// </summary>
        [FlagArgument("-sync", order: 1)]
        public bool DoSync { get; set; }
        /// <inheritdoc cref="DfCommand{TCommandResult}"/>
        public DfCommand() : base(LinuxCommandConstants.Commands.Df)
        {

        }
        /// <inheritdoc/>
        protected override IEnumerable<(string Argument, int Order)> GetStaticArguments(IEnumerable<ILogger>? loggers = null)
        {
            yield return ("-P", 1);
        }
    }

    
}
