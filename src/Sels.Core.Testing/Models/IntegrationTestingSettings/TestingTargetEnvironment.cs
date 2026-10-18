namespace Sels.Core.Testing
{
    /// <summary>Defines whether integration tests use an internal host or a live environment.</summary>
    public enum TestingTargetEnvironment
    {
        /// <summary>Uses an internal testing host.</summary>
        Internal,

        /// <summary>Tests an already running environment.</summary>
        Live
    }
}
