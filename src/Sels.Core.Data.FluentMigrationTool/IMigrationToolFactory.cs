using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Data.FluentMigrationTool
{
    /// <summary>
    /// Factory that creates new builders for creating migration tools.
    /// </summary>
    public interface IMigrationToolFactory
    {
        /// <summary>
        /// Returns a new builder for creating a migration tool.
        /// </summary>
        /// <param name="selfContained">True if the builder should not inherit service registration from the global service collection. otherwise false not to inherit</param>
        /// <returns>Builder for creating anew migration tool</returns>
        IMigrationToolRootBuilder Create(bool selfContained = false);
    }
}
