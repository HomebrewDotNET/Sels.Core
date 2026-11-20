using System.IO.Abstractions;
using Sels.Core.Extensions.IO;
using Sels.Core.Testing;
using Sels.Core.Testing.Templates;

namespace Sels.Core.Legacy.IntegrationTests.Extensions.IO
{
    /// <summary>Contract tests for the file system abstraction overloads of <see cref="FileSystemInfoExtensions"/>.</summary>
    [Trait("Category", "Integration")]
    public class FileSystemInfoExtensionsIntegrationTests : BaseIntegrationTest<IntegrationTestingSettings>
    {
        /// <summary>The drive returned for a directory is the drive containing its path.</summary>
        [Fact]
        public void GetDriveInfo_Directory_ReturnsDriveOfPathRoot()
        {
            IFileSystemInfo info = new FileSystem().DirectoryInfo.New(Path.GetTempPath());

            var drive = info.GetDriveInfo();

            Assert.Equal(Path.GetPathRoot(info.FullName), drive.Name);
        }
    }
}
