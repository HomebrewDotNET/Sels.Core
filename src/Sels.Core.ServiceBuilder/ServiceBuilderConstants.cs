namespace Sels.Core.ServiceBuilder
{
    /// <summary>Contains constants used by service registration.</summary>
    public static class ServiceBuilderConstants
    {
        /// <summary>Contains messages associated with registration.</summary>
        public static class Registration
        {
            /// <summary>The error message for unsupported external-instance configuration.</summary>
            public const string ExternalInstanceConfigurationMessage = "External instances require singleton lifetime, no key and no interceptors";
        }
    }
}