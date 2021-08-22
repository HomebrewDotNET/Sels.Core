using Sels.Core.Components.Conversion;
using Sels.Core.Components.Serialization.Table;
using Sels.Core.Components.Serialization.Table.Attributes;
using Sels.Core.Contracts.Conversion;
using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using Sels.Core.Linux.Templates.LinuxCommand;
using Sels.Core.Templates.FileSizes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sels.Core.Linux.Templates.LinuxCommand.Commands.FileSystem
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

        public DfCommand() : base(LinuxConstants.Commands.Df)
        {

        }

        protected override IEnumerable<(string Argument, int Order)> GetStaticArguments()
        {
            yield return ("-P", 1);
        }
    }

    
}
