using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Command.Linux.Templates.Commands.PackageManager;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Command.Linux.Attributes;
using Sels.Core.Conversion.Attributes.KeyValue;
using Sels.Core.Conversion.Serializers.KeyValue;
using Sels.Core.Extensions.Conversion;

namespace Sels.Core.Command.Linux.Commands.PackageManager
{
    /// <summary>
    /// Used to list package information on Debian/Ubuntu systems.
    /// </summary>
    public class DpkgInfoCommand : DpkgCommand<PackageInfo>
    {
        /// <summary>
        /// The name of the package to get more info about.
        /// </summary>
        [TextArgument(order: 1, prefix: "-s", required: true)]
        public string PackageName { get; set; }
        /// <inheritdoc cref="DpkgInfoCommand"/>
        public DpkgInfoCommand(string packageName)
        {
            PackageName = packageName;
        }
        /// <inheritdoc cref="DpkgInfoCommand"/>
        public DpkgInfoCommand()
        {

        }
        /// <inheritdoc/>
        public override PackageInfo CreateResult(bool wasSuccesful, int exitCode, string? output, string? error, IEnumerable<ILogger>? loggers = null)
        {
            if (wasSuccesful)
            {
                var serializer = new KeyValueSerializer(x => x.UseLoggers(loggers.ToArrayOrDefault()));
                return serializer.Deserialize<PackageInfo>(output);
            }
            else
            {
                return new PackageInfo()
                {
                    Package = PackageName,
                    Status = "not installed"
                };
            }
        }
    }

    /// <summary>
    /// Contains information about a dpkg package.
    /// </summary>
    public class PackageInfo
    {
        // Constants
        private const string SuccessfulInstallStatus = "install ok installed";

        // Properties
        public string Package { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string Section { get; set; }
        [Key("Installed-Size")]
        public string InstalledSize { get; set; }
        public string Maintainer { get; set; }
        public string Architecture { get; set; }
        public string Version { get; set; }
        [Key("Original-Maintainer")]
        public int OriginalMaintainer { get; set; }

        public bool IsInstalled => Status.HasValue() && Status.Equals(SuccessfulInstallStatus);
    }
}
