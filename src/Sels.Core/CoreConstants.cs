namespace Sels.Core
{
    /// <summary>Contains shared runtime-tier constants.</summary>
    public static class CoreConstants
    {
        /// <summary>Contains configuration keys, defaults, and messages.</summary>
        public static class Configuration
        {
            /// <summary>The .NET environment variable name.</summary>
            public const string DotNetEnvironmentKey = "DOTNET_ENVIRONMENT";

            /// <summary>The ASP.NET Core environment variable name.</summary>
            public const string AspNetCoreEnvironmentKey = "ASPNETCORE_ENVIRONMENT";

            /// <summary>The default environment name.</summary>
            public const string DefaultEnvironmentName = "Production";

            /// <summary>The default JSON configuration filename.</summary>
            public const string DefaultSettingsFileName = "appsettings.json";

            /// <summary>The format string for environment-specific configuration files.</summary>
            public const string EnvironmentSettingsFileNameFormat = "appsettings.{0}.json";

            /// <summary>The exception message for repeated startup configuration assignment.</summary>
            public const string AlreadyInitializedMessage = "Startup configuration has already been set.";
        }

        /// <summary>Contains names used to build runtime expressions.</summary>
        public static class Expressions
        {
            /// <summary>The expression parameter name for input objects.</summary>
            public const string InputParameterName = "input";

            /// <summary>The local variable name for a cast input object.</summary>
            public const string CastInputVariableName = "castInput";

            /// <summary>The local variable name for a generated hash code.</summary>
            public const string HashCodeVariableName = "hashCode";
        }
    }
}