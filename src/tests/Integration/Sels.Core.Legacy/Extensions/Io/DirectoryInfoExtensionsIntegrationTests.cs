using System.IO.Abstractions;
using Sels.Core.Extensions.IO;
using Sels.Core.Testing;
using Sels.Core.Testing.Templates;

namespace Sels.Core.Legacy.IntegrationTests.Extensions.IO
{
    /// <summary>Contract tests for the file system abstraction overloads of <see cref="DirectoryInfoExtensions"/>.</summary>
    [Trait("Category", "Integration")]
    public class DirectoryInfoExtensionsIntegrationTests : BaseIntegrationTest<IntegrationTestingSettings>, IDisposable
    {
        private readonly IFileSystem _fileSystem = new FileSystem();
        private readonly string _rootPath;
        private readonly IDirectoryInfo _testDirectory;

        /// <summary>Creates a unique directory for the test to work in.</summary>
        public DirectoryInfoExtensionsIntegrationTests()
        {
            _rootPath = Path.Combine(Path.GetTempPath(), "Sels.Core.Legacy.IntegrationTests", Guid.NewGuid().ToString("N"));
            _testDirectory = _fileSystem.DirectoryInfo.New(_rootPath);
            _testDirectory.Create();
        }

        /// <summary>Removes the directory created by the test.</summary>
        public void Dispose()
        {
            _fileSystem.DirectoryInfo.New(_rootPath).Delete(true);
        }

        /// <summary>Creating a directory that does not exist creates it.</summary>
        [Fact]
        public void CreateIfNotExist_MissingDirectory_CreatesDirectory()
        {
            var directory = GetDirectory("created");

            directory.CreateIfNotExist();

            Assert.True(GetDirectory("created").Exists);
        }

        /// <summary>Ensuring the path of a nested directory creates the missing parents.</summary>
        [Fact]
        public void EnsurePath_NestedDirectory_CreatesParents()
        {
            var directory = GetDirectory(Path.Combine("parent", "child"));

            directory.EnsurePath();

            Assert.True(GetDirectory("parent").Exists);
            Assert.True(GetDirectory(Path.Combine("parent", "child")).Exists);
        }

        /// <summary>A directory without files or sub directories is reported as empty.</summary>
        [Fact]
        public void IsEmpty_EmptyDirectory_ReturnsTrue()
        {
            Assert.True(_testDirectory.IsEmpty());
        }

        /// <summary>A directory containing a file is not reported as empty.</summary>
        [Fact]
        public void IsEmpty_DirectoryWithFile_ReturnsFalse()
        {
            CreateFile(_testDirectory, "file.txt");

            Assert.False(_testDirectory.IsEmpty());
        }

        /// <summary>Clearing a directory deletes the files in its whole tree.</summary>
        [Fact]
        public void Clear_DirectoryWithContent_DeletesFiles()
        {
            CreateFile(_testDirectory, "file.txt");
            var subDirectory = CreateDirectory("sub");
            CreateFile(subDirectory, "nested.txt");

            _testDirectory.Clear();

            Assert.False(GetFile(_testDirectory, "file.txt").Exists);
            Assert.False(GetFile(subDirectory, "nested.txt").Exists);
        }

        /// <summary>Copying with the concrete overload creates the missing parents of the destination.</summary>
        [Fact]
        public void CopyTo_ConcreteDirectoryWithMissingParent_CreatesDestinationParents()
        {
            var source = CreateDirectory("concrete-source");
            CreateFile(source, "file.txt");
            var destination = new DirectoryInfo(Path.Combine(_rootPath, "missing", "parent"));

            var copy = new DirectoryInfo(source.FullName).CopyTo(destination);

            Assert.Equal(Path.Combine(destination.FullName, source.Name), copy.FullName);
            Assert.True(File.Exists(Path.Combine(copy.FullName, "file.txt")));
        }

        /// <summary>The concrete and abstraction overloads copy the same tree.</summary>
        [Fact]
        public void CopyTo_ConcreteAndAbstractionOverloads_CopySameTree()
        {
            var source = CreateDirectory("parity-source");
            CreateFile(source, "file.txt");
            CreateFile(CreateDirectory("parity-source/sub"), "nested.txt");
            var concreteDestination = new DirectoryInfo(Path.Combine(_rootPath, "concrete"));
            var abstractionDestination = CreateDirectory("abstraction");

            var concreteCopy = new DirectoryInfo(source.FullName).CopyTo(concreteDestination);
            var abstractionCopy = source.CopyTo(abstractionDestination);

            Assert.Equal(concreteCopy.Name, abstractionCopy.Name);
            Assert.Equal(Directory.GetFiles(concreteCopy.FullName).Select(Path.GetFileName), abstractionCopy.GetFiles().Select(x => x.Name));
            Assert.Equal(Directory.GetDirectories(concreteCopy.FullName, "*", SearchOption.AllDirectories).Length, abstractionCopy.GetDirectories("*", SearchOption.AllDirectories).Length);
        }

        /// <summary>The relative directory is resolved below the directory it is requested from.</summary>
        [Fact]
        public void GetRelative_RelativePath_ReturnsSubDirectory()
        {
            var relative = _testDirectory.GetRelative(Path.Combine("first", "second"));

            Assert.Equal(Path.Combine(_testDirectory.FullName, "first", "second"), relative.FullName);
        }

        /// <summary>Copying a directory copies its files and sub directories below the destination.</summary>
        [Fact]
        public void CopyTo_Directory_CopiesFilesAndSubDirectories()
        {
            var source = CreateDirectory("source");
            CreateFile(source, "file.txt");
            CreateFile(CreateDirectory(Path.Combine("source", "sub")), "nested.txt");
            var destination = CreateDirectory("destination");

            var copy = source.CopyTo(destination);

            Assert.Equal(Path.Combine(destination.FullName, source.Name), copy.FullName);
            Assert.True(GetFile(copy, "file.txt").Exists);
            Assert.True(GetFile(copy, Path.Combine("sub", "nested.txt")).Exists);
        }

        /// <summary>Copying the content of a directory copies it into the destination directory itself.</summary>
        [Fact]
        public void CopyContentTo_Directory_CopiesContentIntoDestination()
        {
            var source = CreateDirectory("source");
            CreateFile(source, "file.txt");
            var destination = CreateDirectory("destination");

            var copy = source.CopyContentTo(destination);

            Assert.Equal(destination.FullName, copy.FullName);
            Assert.True(GetFile(destination, "file.txt").Exists);
        }

        private IDirectoryInfo GetDirectory(string relativePath) => _fileSystem.DirectoryInfo.New(Path.Combine(_rootPath, relativePath));

        private IDirectoryInfo CreateDirectory(string relativePath)
        {
            var directory = GetDirectory(relativePath);
            directory.Create();
            return directory;
        }

        private static IFileInfo GetFile(IDirectoryInfo directory, string relativePath) => directory.FileSystem.FileInfo.New(Path.Combine(directory.FullName, relativePath));

        private static void CreateFile(IDirectoryInfo directory, string relativePath) => directory.FileSystem.File.WriteAllText(Path.Combine(directory.FullName, relativePath), "content");
    }
}
