using Sels.Core.Extensions.Text;
using System.Text;

namespace Sels.Core.Cli.ArgumentParsing
{
    /// <summary>
    /// Thrown when invalid arguments were passed to the cli.
    /// </summary>
    public class InvalidCommandLineArgumentsException : CommandLineException
    {
        private const string MessageFormat = "Invalid arguments passed: {0}";

        /// <inheritdoc cref="InvalidCommandLineArgumentsException"/>
        /// <param name="reason">The reason why the arguments are invalid</param>
        public InvalidCommandLineArgumentsException(string reason) : base(CommandLine.InvalidArgumentsExitCode, MessageFormat.FormatString(reason.ValidateArgumentNotNullOrWhitespace(nameof(reason))))
        {
        }
        /// <inheritdoc cref="InvalidCommandLineArgumentsException"/>
        /// <param name="errors">A list of errors why the arguments are invalid</param>
        public InvalidCommandLineArgumentsException(IEnumerable<string> errors) :base(CommandLine.InvalidArgumentsExitCode, MessageFormat.FormatString(CreateErrorMessage(errors)))
        {
        }

        private static string CreateErrorMessage(IEnumerable<string> errors)
        {
            errors.ValidateArgumentNotNullOrEmpty(nameof(errors));

            var builder = new StringBuilder();

            builder.AppendLine();

            foreach (var error in errors)
            {
                builder.Append('-').AppendSpace().AppendLine(error);
            }

            return builder.ToString();
        }
    }
}
