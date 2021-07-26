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

namespace Sels.Core.Linux.Templates.Commands.FileSystem
{
    /// <summary>
    /// Linux command for getting information of disk space 
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

        // Statics
        protected static IGenericTypeConverter _converter = GenericConverter.DefaultConverter;
        protected static TableSerializer _serializer = new TableSerializer(_converter, Environment.NewLine, skipDataRow: 1);
    }

    
}
