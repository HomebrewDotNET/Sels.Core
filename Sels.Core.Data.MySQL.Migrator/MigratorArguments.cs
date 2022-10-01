using CommandLine;
using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.MySQL.Migrator
{
    /// <summary>
    /// Command line arguments that can be provided to the MySql migration runner tool.
    /// </summary>
    public class MigratorArguments
    {
        /// <summary>
        /// The connection string to use for deploying the changes.
        /// </summary>
        [Option('c', Required = true)]
        public string? ConnectionString { get; set; }
        /// <summary>
        /// Log more about what the runner is doing.
        /// </summary>
        [Option('v')]
        public bool Verbose { get; set; }
        /// <summary>
        /// Logs more for debugging.
        /// </summary>
        [Option('d')]
        public bool Debug { get; set; }
    }
}
