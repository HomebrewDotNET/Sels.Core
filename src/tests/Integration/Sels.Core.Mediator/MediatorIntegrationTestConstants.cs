namespace Sels.Core.Mediator.IntegrationTests
{
    /// <summary>Timeout values used to bound asynchronous integration-test work.</summary>
    public static class MediatorIntegrationTestConstants
    {
        /// <summary>Maximum time in milliseconds an asynchronous integration test may run before xUnit fails it.</summary>
        public const int TestTimeoutMilliseconds = 60000;
    }
}
