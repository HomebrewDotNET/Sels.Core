using Sels.Core.Components.Serialization.KeyValue;
using Sels.Core.Components.Serialization.KeyValue.Attributes;
using Sels.Core.Extensions;
using Sels.Core.Linux.Components.LinuxCommand;
using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using Sels.Core.Linux.Templates.LinuxCommand.Commands.PackageManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Components.LinuxCommand.Commands.PackageManager
{
    /// <summary>
    /// Used to list package information on Debian/Ubuntu systems.
    /// </summary>
    public class DpkgInfoCommand : DpkgCommand<PackageInfo>
    {
        [TextArgument(order: 1, prefix: "-s", required: true)]
        public string PackageName { get; set; }

        public DpkgInfoCommand(string packageName)
        {
            PackageName = packageName;
        }

        public DpkgInfoCommand()
        {

        }

        public override PackageInfo CreateResult(bool wasSuccesful, int exitCode, string output, string error)
        {
            if (wasSuccesful)
            {
                return KeyValueConverter.Deserialize<PackageInfo>(output);
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

    public class PackageInfo
    {
        // Constants
        private const string SuccessfulInstallStatus = "install ok installed";

        // Properties
        public string Package { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string Section { get; set; }
        [KeyValueProperty(key: "Installed-Size")]
        public string InstalledSize { get; set; }
        public string Maintainer { get; set; }
        public string Architecture { get; set; }
        public string Version { get; set; }
        [KeyValueProperty(key: "Original-Maintainer")]
        public int OriginalMaintainer { get; set; }

        public bool IsInstalled => Status.HasValue() && Status.Equals(SuccessfulInstallStatus);
    }
}
