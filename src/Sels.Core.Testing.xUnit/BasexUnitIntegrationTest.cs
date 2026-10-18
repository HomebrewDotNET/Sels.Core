using Sels.Core.Configuration;
using Sels.Core.Testing.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sels.Core.Testing.xUnit
{
    /// <summary>
    /// Base class for integration tests that uses xUnit and a <see cref="ITestingHost"/> to run the tests.
    /// </summary>
    /// <typeparam name="THost">The type of the testing host</typeparam>
    /// <typeparam name="TSetting">The type of the app settings for the integration test</typeparam>
    public abstract class BasexUnitIntegrationTest<THost, TSetting> : BaseIntegrationTest<TSetting>, ICollectionFixture<THost> where TSetting : AppSettings<TSetting>, new() where THost : class, ITestingHost
    {
        /// <inheritdoc cref="BasexUnitIntegrationTest{THost, TSetting}"/>
        /// <param name="host">The testing host to use for the integration test</param>
        protected BasexUnitIntegrationTest(THost host)
        {
            Host = host;
        }

        /// <summary>
        /// The testing host to use for the integration test.
        /// </summary>
        protected ITestingHost IntegrationTestHost => Guard.IsNotNull(Host);
    }
}
