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
        }

    }
}
