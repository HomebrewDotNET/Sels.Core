using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Linux.Contracts.LinuxCommand;
using Sels.Core.Linux.Extensions.Argument;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sels.Core.Linux.Templates.LinuxCommand
{
    /// <summary>
    /// Used to run linux commands or build linux command strings.
    /// </summary>
    public abstract class BaseLinuxCommand : BaseLinuxCommand<string>
    {
        public BaseLinuxCommand(string name) : base(name)
        {

        }
    }

    /// <summary>
    /// Used to run linux commands or build linux command strings.
    /// </summary>
    /// <typeparam name="TName">Type of object that represents the command name</typeparam>
    public abstract class BaseLinuxCommand<TName> : BaseLinuxCommand<TName, string>, ILinuxCommand
    {
        public BaseLinuxCommand(TName name) : base(name)
        {
        }

        public override string CreateResult(bool wasSuccesful, int exitCode, string output, string error)
        {
            return error.HasValue() ? error : output;
        }
    }

    /// <summary>
    /// Used to run linux commands or build linux command strings.
    /// </summary>
    /// <typeparam name="TName">Type of object that represents the command name</typeparam>
    /// <typeparam name="TCommandResult">Type of result that the command returns</typeparam>
    public abstract class BaseLinuxCommand<TName, TCommandResult> : ILinuxCommand<TCommandResult>
    {
        public TName Name { get; }

        public BaseLinuxCommand(TName name) 
        {
            Name = name.ValidateArgument(nameof(name));
        }

        public virtual bool RunCommand(out string output, out string error, out int exitCode)
        {
            return LinuxHelper.Program.Run(Name.GetArgumentValue(), BuildArguments(), out output, out error, out exitCode, SuccessExitCode);
        }

        public virtual string BuildCommand()
        {
            return $"{Name} {BuildArguments()}";
        }

        /// <summary>
        /// Builds arguments for running the linux command.
        /// </summary>
        protected virtual string BuildArguments()
        {
            return LinuxHelper.Arguments.BuildLinuxArguments(this);
        }

        public TCommandResult Execute()
        {
            return Execute(out _);
        }

        public TCommandResult Execute(out int exitCode)
        {
            if(RunCommand(out string output, out string error, out exitCode))
            {
                return CreateResult(true, exitCode, output, error);
            }
            else
            {
                return CreateResult(false, exitCode, output, error);
            }
        }

        /// <summary>
        /// Exit code returned from executing the command that indicates it executed succesfully.
        /// </summary>
        public virtual int SuccessExitCode => LinuxConstants.SuccessExitCode;

        public abstract TCommandResult CreateResult(bool wasSuccesful, int exitCode, string output, string error);
    }
}
