using Sels.Core.Cli.ArgumentParsing;
using Sels.Core.Extensions.Linq;

namespace Sels.Core.Cli.Tests
{
    /// <summary>Contract tests for <see cref="CommandLine"/>.</summary>
    [Trait("Category", "Unit")]
    public class CommandLineTests
    {
        /// <summary>Verifies ArgumentParse for short flag arguments: parses expected flags.</summary>
        [Theory]
        [InlineData(new string[] { "-Rfv", "config", "deploy", "-T", "/temp/output/git", "-p", "ConnectionString=SomeConnectionString", "-p", "Port=5000", "-D" }, new char[] { 'R', 'f', 'D' })]
        [InlineData(new string[] { "add", "-q", "-C", "/temp" }, new char[] { 'q' })]
        [InlineData(new string[] { "build", "-c", "Release", "-a", "--action=Build,Clean,Restore" }, new char[] { 'a' })]
        [InlineData(new string[] { "-QfKL" }, new char[] { 'L', 'f', 'K', 'Q' })]
        [InlineData(new string[] { "-h" }, new char[] { 'h' })]
        [InlineData(new string[] { "-y", "-d", "-r" }, new char[] { 'y', 'd', 'r' })]
        public void ArgumentParse_ShortFlagArguments_ParsesExpectedFlags(string[] args, char[] flags)
        {
            // Arrange
            List<char> parsedFlags = new List<char>();
            Action<IArgumentParserBuilder<NullArguments>> builder = builder => {
                flags.Execute(flag =>
                {
                    builder.SetValue<bool>(arg => parsedFlags.Add(flag)).FromOption(flag);
                });
            };

            // Act
            var result = CommandLine.Argument.Parse(builder, args, settings: ArgumentParserSettings.None);

            // Assert
            Assert.Equal(flags.OrderBy(x => x), parsedFlags.OrderBy(x => x));
        }
    }
}
