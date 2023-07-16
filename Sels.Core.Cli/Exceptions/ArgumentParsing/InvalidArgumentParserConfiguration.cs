using Sels.Core.Extensions.Text;

namespace Sels.Core.Cli.ArgumentParsing
{
    /// <summary>
    /// Thrown when an argument parser has invalid configuration.
    /// </summary>
    public class InvalidArgumentParserConfiguration : CommandLineException
    {
        private const string MessageFormat = "Invalid argument parser configuration: {0}";

        /// <inheritdoc cref="InvalidArgumentParserConfiguration"/>
        /// <param name="reason">The reason why the configuration is invalid</param>
        public InvalidArgumentParserConfiguration(string reason) : base(1, MessageFormat.FormatString(reason.ValidateArgumentNotNullOrWhitespace(nameof(reason))))
        {

        }   
    }
}
