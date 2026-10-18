using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Testing
{
    /// <summary>
    /// Empty implementation of <see cref="ITestingHost"/> that can be used when a test doesn't require a testing host, but the test template requires one. This allows for tests to be executed without the need for a testing host, while still adhering to the expected structure of the test template. The <see cref="NullHost"/> can be used as a placeholder or default implementation of <see cref="ITestingHost"/> in scenarios where a testing host is not necessary or relevant to the test being executed.
    /// </summary>
    public sealed class NullHost : ITestingHost
    {
        /// <inheritdoc/>
        public HttpClient CreateAppClientAsync(string resource, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
