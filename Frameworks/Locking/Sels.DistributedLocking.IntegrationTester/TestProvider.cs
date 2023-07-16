using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.IntegrationTester
{
    /// <summary>
    /// Dictates which provider to test.
    /// </summary>
    [Flags]
    public enum TestProvider
    {
        /// <summary>
        /// Test all providers.
        /// </summary>
        All = Memory | MySql | MariaDb,
        /// <summary>
        /// Test the in-memory provider.
        /// </summary>
        Memory = 1,
        /// <summary>
        /// Test the MySql provider.
        /// </summary>
        MySql = 2,
        // <summary>
        /// Test the MySql provider optimized for MariaDb.
        /// </summary>
        MariaDb = 4
    }
}
