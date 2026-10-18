namespace Sels.Core.ServiceBuilder.IntegrationTests
{
    /// <summary>Timeout values used to bound asynchronous integration-test work.</summary>
    public static class ServiceBuilderIntegrationTestConstants
    {
        /// <summary>Maximum time in milliseconds an asynchronous integration test may run before xUnit fails it.</summary>
        public const int TestTimeoutMilliseconds = 60000;
    }
}
