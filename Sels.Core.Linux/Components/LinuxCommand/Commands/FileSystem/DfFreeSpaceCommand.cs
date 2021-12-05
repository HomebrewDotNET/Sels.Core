﻿using Microsoft.Extensions.Logging;
using Sels.Core.Contracts.Commands;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using Sels.Core.Linux.Components.LinuxCommand.Commands.Awk;
using Sels.Core.Linux.Contracts.LinuxCommand;
using Sels.Core.Linux.Contracts.LinuxCommand.Commands;
using Sels.Core.Linux.Templates.LinuxCommand;
using Sels.Core.Templates.FileSizes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Commands.FileSystem
{
    /// <summary>
    /// Command that checks how much free disk space a file system member has.
    /// </summary>
    /// <typeparam name="TFileSize">Type of filesize format to display the free disk space in</typeparam>
    public class DfFreeSpaceCommand<TFileSize> : MultiCommand<ILinuxCommandResult<TFileSize, string>> where TFileSize : FileSize
    {
        // Constants
        private const string _awkScript = "{print $4}";

        // Fields
        private readonly DfFileSizeConverter _converter = new();
        private readonly DfInfoCommand _infoCommand = new();
        private readonly DynamicAwkCommand _awkCommand = new(_awkScript);

        // Properties
        /// <summary>
        /// File system member to get info about.
        /// </summary>
        public FileSystemInfo Member { get { return _infoCommand.Member; } set { _infoCommand.Member = value; } }

        /// <summary>
        /// Command that checks how much free disk space a file system member has.
        /// </summary>
        public DfFreeSpaceCommand()
        {

        }

        /// <inheritdoc/>
        public override ILinuxCommandResult<TFileSize, string> CreateResult(bool wasSuccesful, int exitCode, string output, string error, IEnumerable<ILogger> loggers = null)
        {
            try
            {
                if (wasSuccesful)
                {
                    var lines = output.SplitStringOnNewLine();

                    if(lines.Length >= 2)
                    {
                        var fileSize = _converter.ConvertTo(typeof(string), typeof(TFileSize), lines[1]).As<TFileSize>();
                        return new LinuxCommandResult<TFileSize, string>(fileSize, exitCode);
                    }
                    else
                    {
                        throw new IOException($"Empty output received from {Name}");
                    }
                }

                return new LinuxCommandResult<TFileSize, string>(error, exitCode);
            }
            catch (IOException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new IOException("Unknow IO error occurred. Could be related to network issues.", ex);
            }           
        }
        /// <inheritdoc/>
        protected override IMultiCommandChain<CommandChainer> BuildCommandChain(IMultiCommandStartSetup<CommandChainer> chainSetup)
        {
            return chainSetup.StartWith(_infoCommand).EndWith(CommandChainer.Pipe, _awkCommand);
        }
    }
}
