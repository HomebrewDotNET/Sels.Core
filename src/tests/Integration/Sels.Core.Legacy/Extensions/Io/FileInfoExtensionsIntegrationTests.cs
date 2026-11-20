using System.IO.Abstractions;
using Sels.Core.Extensions.IO;
using Sels.Core.Testing;
using Sels.Core.Testing.Templates;

namespace Sels.Core.Legacy.IntegrationTests.Extensions.IO
{
    /// <summary>Contract tests for the file system abstraction overloads of <see cref="FileInfoExtensions"/>.</summary>
    [Trait("Category", "Integration")]
    public class FileInfoExtensionsIntegrationTests : BaseIntegrationTest<IntegrationTestingSettings>, IDisposable
    {
        private readonly IFileSystem _fileSystem = new FileSystem();
        private readonly string _rootPath;

        /// <summary>Creates a unique directory for the test to work in.</summary>
        public FileInfoExtensionsIntegrationTests()
        {
            _rootPath = Path.Combine(Path.GetTempPath(), "Sels.Core.Legacy.IntegrationTests", Guid.NewGuid().ToString("N"));
            _fileSystem.DirectoryInfo.New(_rootPath).Create();
        }

        /// <summary>Removes the directory created by the test.</summary>
        public void Dispose()
        {
            _fileSystem.DirectoryInfo.New(_rootPath).Delete(true);
        }

        /// <summary>Writing and reading through the abstraction returns the written content.</summary>
        [Fact]
        public void Write_ContentProvided_WritesContent()
        {
            const string content = "content";

            GetFile("file.txt").Write(content);

            var written = GetFile("file.txt");
            Assert.True(written.Exists);
            Assert.Equal(content, written.Read());
        }

        /// <summary>Writing and reading asynchronously through the abstraction returns the written content.</summary>
        [Fact]
        public async Task WriteAsync_ContentProvided_WritesContent()
        {
            const string content = "asynchronous content";

            await GetFile("async.txt").WriteAsync(content);

            Assert.Equal(content, await GetFile("async.txt").ReadAsync());
        }

        /// <summary>Clearing a file keeps it on disk but removes its content.</summary>
        [Fact]
        public void Clear_WrittenFile_EmptiesContent()
        {
            var file = GetFile("clear.txt");
            file.Write("content");

            file.Clear();

            var cleared = GetFile("clear.txt");
            Assert.True(cleared.Exists);
            Assert.Equal(string.Empty, cleared.Read());
        }

        /// <summary>Creating an existing file with overwrite replaces its content and reports the file as created.</summary>
        [Fact]
        public void Create_ExistingFileWithOverwrite_ReplacesContent()
        {
            var file = GetFile("create.txt");
            file.Write("first");

            var created = file.Create("second", overwrite: true);

            Assert.True(created);
            Assert.Equal("second", GetFile("create.txt").Read());
        }

        /// <summary>Creating an existing file without overwrite leaves the content untouched and reports no creation.</summary>
        [Fact]
        public void Create_ExistingFileWithoutOverwrite_KeepsContent()
        {
            var file = GetFile("create.txt");
            file.Write("first");

            var created = file.Create("second");

            Assert.False(created);
            Assert.Equal("first", GetFile("create.txt").Read());
        }

        /// <summary>Copying a file to an abstraction directory copies it into that directory.</summary>
        [Fact]
        public void CopyTo_Directory_CopiesFileIntoDirectory()
        {
            const string content = "content";
            var file = GetFile("copy.txt");
            file.Write(content);
            var destination = CreateDirectory("copied");

            var copy = file.CopyTo(destination);

            Assert.Equal(Path.Combine(destination.FullName, file.Name), copy.FullName);
            Assert.Equal(content, GetFile(copy.FullName).Read());
            Assert.True(GetFile("copy.txt").Exists);
        }

        /// <summary>Copying a file with a supplied name copies it under that name.</summary>
        [Fact]
        public void CopyTo_DirectoryWithFileName_UsesSuppliedName()
        {
            var file = GetFile("named.txt");
            file.Write("content");
            var destination = CreateDirectory("copied");

            var copy = file.CopyTo(destination, "renamed.txt");

            Assert.Equal("renamed.txt", copy.Name);
            Assert.True(GetFile(copy.FullName).Exists);
        }

        /// <summary>Moving a file to an abstraction directory moves it into that directory.</summary>
        [Fact]
        public void MoveTo_Directory_MovesFileIntoDirectory()
        {
            var file = GetFile("moved.txt");
            file.Write("content");
            var destination = CreateDirectory("moved");

            var moved = file.MoveTo(destination);

            Assert.Equal(Path.Combine(destination.FullName, file.Name), moved.FullName);
            Assert.True(GetFile(moved.FullName).Exists);
            Assert.False(GetFile("moved.txt").Exists);
        }

        /// <summary>Moving a file with a supplied name moves it under that name.</summary>
        [Fact]
        public void MoveTo_DirectoryWithFileName_UsesSuppliedName()
        {
            var file = GetFile("named.txt");
            file.Write("content");
            var destination = CreateDirectory("moved");

            var moved = file.MoveTo(destination, "renamed.txt");

            Assert.Equal("renamed.txt", moved.Name);
            Assert.False(GetFile("named.txt").Exists);
        }

        /// <summary>A file opened exclusively by the test is reported as locked.</summary>
        [Fact]
        public void IsLocked_FileOpenedExclusively_ReturnsTrue()
        {
            GetFile("locked.txt").Write("content");
            var file = GetFile("locked.txt");

            using (var stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                Assert.True(file.IsLocked());
            }
        }

        /// <summary>The extension of a file is returned without the leading dot.</summary>
        [Fact]
        public void GetExtensionName_FileWithMultipleDots_ReturnsExtensionWithoutDot()
        {
            GetFile("part.name.txt").Write("content");

            Assert.Equal("txt", GetFile("part.name.txt").GetExtensionName());
        }

        /// <summary>The file name without its extension is returned for a file.</summary>
        [Fact]
        public void GetNameWithoutExtension_FileWithMultipleDots_ReturnsNameWithoutExtension()
        {
            GetFile("part.name.txt").Write("content");

            Assert.Equal("part.name", GetFile("part.name.txt").GetNameWithoutExtension());
        }

        private IFileInfo GetFile(string name) => _fileSystem.FileInfo.New(Path.Combine(_rootPath, name));

        private IDirectoryInfo CreateDirectory(string name)
        {
            var directory = _fileSystem.DirectoryInfo.New(Path.Combine(_rootPath, name));
            directory.Create();
            return directory;
        }
    }
}
