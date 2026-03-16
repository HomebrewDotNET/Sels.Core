namespace Sels.Core.Command.Linux
{
    /// <summary>
    /// Contains constants for linux command.
    /// </summary>
    public static class LinuxCommandConstants
    {
        /// <summary>
        /// Contains constants for common linux commands.
        /// </summary>
        public static class Commands
        {
            private const string DefaultCommandRoot = "/bin/";
            /// <summary>
            /// The name of the sudo command.
            /// </summary>
            public const string Sudo = DefaultCommandRoot + "sudo";
            /// <summary>
            /// The name of the echo command.
            /// </summary>
            public const string Echo = DefaultCommandRoot + "echo";
            /// <summary>
            /// The name of the shell command.
            /// </summary>
            public const string Shell = DefaultCommandRoot + "sh";
            /// <summary>
            /// The name of the bash command.
            /// </summary>
            public const string Bash = DefaultCommandRoot + "bash";
            /// <summary>
            /// The name of the dpkg command.
            /// </summary>
            public const string Dpkg = DefaultCommandRoot + "dpkg";
            /// <summary>
            /// The name of the grep command.
            /// </summary>
            public const string Grep = DefaultCommandRoot + "grep";
            /// <summary>
            /// The name of the tee command.
            /// </summary>
            public const string Tee = DefaultCommandRoot + "tee";
            /// <summary>
            /// The name of the screen command.
            /// </summary>
            public const string Screen = DefaultCommandRoot + "screen";
            /// <summary>
            /// The name of the df command.
            /// </summary>
            public const string Df = DefaultCommandRoot + "df";
            /// <summary>
            /// The name of the awk command.
            /// </summary>
            public const string Awk = DefaultCommandRoot + "awk";
        }

        /// <summary>
        /// The default order that dictates where linux command arguments are placed in the command string.
        /// </summary>
        public const int DefaultLinuxArgumentOrder = -1;
    }
}
