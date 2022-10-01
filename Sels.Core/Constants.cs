using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core
{
    /// <summary>
    /// Contains constant/static values used by other components.
    /// </summary>
    public static class Constants
    {
        #region String
        /// <summary>
        /// Contains constant string values.
        /// </summary>
        public static class Strings
        {
            /// <summary>
            /// Contains a space character.
            /// </summary>
            public const string Space = " ";
            /// <summary>
            /// Contains a tab character.
            /// </summary>
            public const string Tab = "\t";
            /// <summary>
            /// Contains a comma character.
            /// </summary>
            public const string Comma = ",";
        }
        #endregion

        /// <summary>
        /// Contains constant values for application configuration.
        /// </summary>
        public static class Configuration
        {
            /// <summary>
            /// The filename of the default configuration file for most .NET applications.
            /// </summary>
            public const string DefaultAppSettingsFile = "appsettings.json";

            /// <summary>
            /// Contains constants for configuration sections.
            /// </summary>
            public static class Sections
            {
                /// <summary>
                /// The name of the default app settings section.
                /// </summary>
                public const string AppSettings = "AppSettings";
                /// <summary>
                /// The name of the default connection strings section.
                /// </summary>
                public const string ConnectionStrings = "ConnectionStrings";
            }
            /// <summary>
            /// Config settings related to logging
            /// </summary>
            public static class Logging
            {
                /// <summary>
                /// The logging section name.
                /// </summary>
                public const string Name = "Logging";
                /// <summary>
                /// The directory where log files are placed.
                /// </summary>
                public const string Directory = "Directory";
                /// <summary>
                /// The directory where old log files are placed.
                /// </summary>
                public const string ArchiveDirectory = "ArchiveDirectory";
                /// <summary>
                /// The max file size of log files before a new one is created.
                /// </summary>
                public const string MaxFileSize = "MaxFileSize";

                /// <summary>
                /// The log section related to the log levels of the various logging categories.
                /// </summary>
                public static class LogLevel
                {
                    /// <summary>
                    /// The log level section name.
                    /// </summary>
                    public const string Name = "LogLevel";
                    /// <summary>
                    /// The name of the default log category used for non-defined categories.
                    /// </summary>
                    public const string Default = "Default";
                    /// <summary>
                    /// The name of microsoft logging category.
                    /// </summary>
                    public const string Microsoft = "Microsoft";
                    /// <summary>
                    /// The name of microsoft logging category and all it sub directories.
                    /// </summary>
                    public const string MicrosoftAll = Microsoft + "*";
                }
            }
        }

    }
}
