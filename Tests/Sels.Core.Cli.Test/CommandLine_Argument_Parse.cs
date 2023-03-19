using System;
using System.Collections.Generic;

namespace Sels.Core.Cli.Test
{
    internal class CommandLine_Argument_Parse
    {
        [TestCase(new string[] { "-Rfv", "config", "deploy", "-T", "/temp/output/git", "-p", "ConnectionString=SomeConnectionString", "-p", "Port=5000", "-D" }, new char[] { 'R', 'f', 'D' })]
        [TestCase(new string[] { "add", "-q", "-C", "/temp" }, new char[] { 'q' })]
        [TestCase(new string[] { "build", "-c", "Release", "-a", "--action=Build,Clean,Restore" }, new char[] { 'a' })]
        [TestCase(new string[] { "-QfKL" }, new char[] { 'L', 'f', 'K', 'Q' })]
        [TestCase(new string[] { "-h" }, new char[] { 'h' })]
        [TestCase(new string[] { "-y", "-d", "-r" }, new char[] { 'y', 'd', 'r' })]
        public void ParsesCorrectShortFlagsFromArgumentList(string[] args, char[] flags)
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
            CollectionAssert.AreEquivalent(flags, parsedFlags);
        }
    }
}
