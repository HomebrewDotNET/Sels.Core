using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux
{
    public static class LinuxConstants
    {
        public static class Commands
        {
            private const string DefaultCommandRoot = "/bin/";

            public const string Sudo = DefaultCommandRoot + "sudo";
            public const string Echo = DefaultCommandRoot + "echo";
            public const string Shell = DefaultCommandRoot + "sh";
            public const string Bash = DefaultCommandRoot + "bash";
            public const string Dpkg = DefaultCommandRoot + "dpkg";
            public const string Grep = DefaultCommandRoot + "grep";
            public const string Tee = DefaultCommandRoot + "tee";
            public const string Screen = DefaultCommandRoot + "screen";
            public const string Df = DefaultCommandRoot + "df";
            public const string Awk = DefaultCommandRoot + "awk";
        }

        public const int SuccessExitCode = 0;

        public const int DefaultLinuxArgumentOrder = -1;
    }
}
