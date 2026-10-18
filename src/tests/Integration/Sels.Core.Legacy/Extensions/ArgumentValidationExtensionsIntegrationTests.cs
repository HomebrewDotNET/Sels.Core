using Sels.Core.Extensions;
using Sels.Core.Testing;
using Sels.Core.Testing.Templates;

namespace Sels.Core.Legacy.IntegrationTests.Extensions
{
    /// <summary>Integration tests for filesystem argument validation.</summary>
    [Trait("Category", "Integration")]
    public class ArgumentValidationExtensionsIntegrationTests : BaseIntegrationTest<IntegrationTestingSettings>
    {
        /// <summary>Filesystem guards accept an embedded fixture and reject absent paths.</summary>
        [Fact]
        public void ValidateArgumentExists_EmbeddedFixtureAndMissingPaths_ValidatesDiskState()
        {
            var fixture = typeof(ArgumentValidationExtensionsIntegrationTests).Assembly
                .GetManifestResourceStream("Sels.Core.Legacy.IntegrationTests.Fixtures.guard-existing.txt");
            Assert.NotNull(fixture);

            var repositoryRoot = FindRepositoryRoot();
            var testDirectory = Path.Combine(repositoryRoot, "temp", "pending-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(testDirectory);
            var existingPath = Path.Combine(testDirectory, "guard-existing.txt");
            var missingPath = Path.Combine(testDirectory, "guard-missing.txt");

            try
            {
                using (var output = File.Create(existingPath))
                {
                    fixture!.CopyTo(output);
                }

                var file = new FileInfo(existingPath);
                var directory = new DirectoryInfo(testDirectory);

                var existingFile = file.ValidateArgumentExists("file");
                var existingDirectory = directory.ValidateArgumentExists("directory");
                var missingFileException = Assert.Throws<ArgumentException>(() => new FileInfo(missingPath).ValidateArgumentExists("file"));

                Assert.Same(file, existingFile);
                Assert.Same(directory, existingDirectory);
                Assert.Contains("file", missingFileException.Message);
            }
            finally
            {
                fixture?.Dispose();
                if (Directory.Exists(testDirectory))
                {
                    Directory.Delete(testDirectory, recursive: true);
                }
            }
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Sels.Core.sln")))
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate the Sels.Core solution root.");
        }
    }
}