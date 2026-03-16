namespace Sels.Core.Command.Linux.Templates.Commands.PackageManager
{
    /// <summary>
    /// Base class for the dpkg command.
    /// </summary>
    /// <typeparam name="TCommandResult">Type of command result</typeparam>
    public abstract class DpkgCommand<TCommandResult> : BaseLinuxCommand<string, TCommandResult>
    {
        /// <inheritdoc cref="DpkgCommand{TCommandResult}"/>
        public DpkgCommand() : base(LinuxCommandConstants.Commands.Dpkg)
        {

        }
    }
}
