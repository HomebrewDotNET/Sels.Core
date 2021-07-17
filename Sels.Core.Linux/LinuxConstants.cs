using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux
{
    public static class LinuxConstants
    {
        public static class Commands
        {
            public const string Sudo = "sudo";
            public const string Echo = "echo";
            public const string Shell = "/bin/sh";
            public const string Bash = "/bin/bash";
            public const string Dpkg = "dpkg";
            public const string Grep = "grep";
            public const string Tee = "tee";
            public const string Screen = "screen";
        }

        public const int SuccessExitCode = 0;

        public const int DefaultLinuxArgumentOrder = -1;
    }
}
