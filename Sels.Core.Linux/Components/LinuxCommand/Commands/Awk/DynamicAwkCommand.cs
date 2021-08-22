using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using Sels.Core.Linux.Templates.LinuxCommand.Commands.Awk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Commands.Awk
{

    public class DynamicAwkCommand : AwkCommand
    {
        // Properties
        /// <summary>
        /// Script text to execute with awk.
        /// </summary>
        [TextArgument(parsingOption: TextParsingOptions.Quotes, allowEmpty:true, order:1, required:true)]
        public string Script { get; set; }
        /// <summary>
        /// Optional file where awk will get it's input from. If null awk will expect the input from a chained command.
        /// </summary>
        [ObjectArgument(Selector.Method, nameof(FileInfo.FullName), order:2, required:false)]
        public FileInfo InputFile { get; set; }

        public DynamicAwkCommand(string script)
        {
            Script = script;
        }

        public DynamicAwkCommand()
        {

        }
    }
}
