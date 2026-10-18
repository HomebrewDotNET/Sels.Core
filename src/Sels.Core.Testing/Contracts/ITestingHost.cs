using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Testing
{
    /// <summary>
    /// A host that manages the lifecycle of a testing environment, providing necessary resources and services for executing tests. This interface can be implemented to create custom testing hosts that set up and tear down test environments, manage dependencies, and provide utilities for test execution. The testing host is responsible for ensuring that tests run in a controlled and isolated environment, allowing for consistent and reliable test results.
    /// </summary>
    public interface ITestingHost : IAsyncDisposable
    {
        /// <summary>
        /// Gets an http client to communicate with <paramref name="resource"/>. Usually this will be the application being tested, but it can be any resource that the testing host provides access to. The returned http client should be configured to communicate with the specified resource, and may include any necessary authentication, headers, or other settings required for successful communication. The testing host is responsible for managing the lifecycle of the http client and ensuring that it is properly disposed of when no longer needed.
        /// </summary>
        /// <param name="resource">The key that identifies the resource to get the client for</param>
        /// <param name="cancellationToken">Token to listed for cancellation requests</param>
        /// <returns>An http client preconfigured so it sends requests to <paramref name="resource"/></returns>
        public HttpClient CreateAppClientAsync(string resource, CancellationToken cancellationToken = default);
    }
}
