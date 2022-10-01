using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Cli.ArgumentParsing
{
    /// <summary>
    /// Contains the result from parsing command line arguments.
    /// </summary>
    /// <typeparam name="T">The type containing the parsed arguments</typeparam>
    public interface IParsedResult<T>
    {
        /// <summary>
        /// The instance created from the parsed arguments.
        /// </summary>
        public T Parsed { get; }
        /// <summary>
        /// The command line arguments this result was parsed from.
        /// </summary>
        public string[] Arguments { get; }
        /// <summary>
        /// Any environment variables that were used to create the result. Key is the environment variable name.
        /// </summary>
        public IReadOnlyDictionary<string, string> ParsedEnvironmentVariables { get;  }
        /// <summary>
        /// The stdin that was parsed.
        /// </summary>
        public string? ParsedInput { get; }
        /// <summary>
        /// All commands that could be parsed. Key is the position the command was parsed from. Value is the string used to parse the command.
        /// </summary>
        public IReadOnlyDictionary<int, string> ParsedCommands { get; }
        /// <summary>
        /// All arguments that could be parsed. Key is the position the command was parsed from. Value is the string used to parse the argument.
        /// </summary>
        public IReadOnlyDictionary<int, string> ParsedArguments { get; }
        /// <summary>
        /// All short options (e.g., -a or /p Action=Build) that could be resolved. Key is the option + prefix. Value are all the string values used to parse the option, will be empty array in case of flags (e.g., -h or -q)
        /// </summary>
        public IReadOnlyDictionary<string, string[]> ParsedOptions { get; }
        /// <summary>
        /// All long options (e.g., --all or /parameter Action=Build). Key is the option + prefix. Value are all the string values used to parse the option, will be empty array in case of flags (e.g., --help or --quiet)
        /// </summary>
        public IReadOnlyDictionary<string, string[]> ParsedLongOptions { get; }
        /// <summary>
        /// Any parsing errors that happened.
        /// </summary>
        public string[] Errors { get; }
        /// <summary>
        /// Any command line arguments that could not be parsed.
        /// </summary>
        public string[] UnparsedArguments { get; }
    }

    /// <summary>
    /// A builder for <see cref="IParsedResult{T}"/>.
    /// </summary>
    public interface IResultBuilder
    {
        /// <summary>
        /// Adds an errors message to the result.
        /// </summary>
        /// <param name="message">The error message to add</param>
        /// <returns>Current builder for method chaining</returns>
        IResultBuilder AddError(string message);
        /// <summary>
        /// Adds the input from stdin that was parsed.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        IResultBuilder AddParsedInput(string input);
        /// <summary>
        /// Adds an environment variable that was parsed from.
        /// </summary>
        /// <param name="name">The environment variable name</param>
        /// <param name="value">The environment variable value</param>
        /// <returns>Current builder for method chaining</returns>
        IResultBuilder AddParsedEnvironmentVariable(string name, string value);
        /// <summary>
        /// Adds a command that was parsed.
        /// </summary>
        /// <param name="position">The position the command was parsed from</param>
        /// <param name="value">The value that was used to parse the command</param>
        /// <returns>Current builder for method chaining</returns>
        IResultBuilder AddParsedCommand(int position, string value);
        /// <summary>
        /// Adds an argument that was parsed.
        /// </summary>
        /// <param name="position">The position the command was parsed from</param>
        /// <param name="argument">The value that was used to parse the argument</param>
        /// <returns>Current builder for method chaining</returns>
        IResultBuilder AddParsedArgument(int position, string argument);
        /// <summary>
        /// Adds a short option that was parsed.
        /// </summary>
        /// <param name="option">The short option that was parsed</param>
        /// <param name="value">The value that was used as an argument to the option, can be null in case of a flag</param>
        /// <returns>Current builder for method chaining</returns>
        IResultBuilder AddParsedOption(string option, string? value = null);
        /// <summary>
        /// Adds a short option that was parsed.
        /// </summary>
        /// <param name="option">The short option that was parsed</param>
        /// <param name="values">An enumerator returning all the values that were used as arguments to the option</param>
        /// <returns>Current builder for method chaining</returns>
        IResultBuilder AddParsedOption(string option, IEnumerable<string> values);
        /// <summary>
        /// Adds a long option that was parsed.
        /// </summary>
        /// <param name="option">The short option that was parsed</param>
        /// <param name="value">The value that was used as an argument to the option, can be null in case of a flag</param>
        /// <returns>Current builder for method chaining</returns>
        IResultBuilder AddParsedLongOption(string option, string? value = null);
        /// <summary>
        /// Adds a long option that was parsed.
        /// </summary>
        /// <param name="option">The short option that was parsed</param>
        /// <param name="values">An enumerator returning all the values that were used as arguments to the option</param>
        /// <returns>Current builder for method chaining</returns>
        IResultBuilder AddParsedLongOption(string option, IEnumerable<string> values);
    }
}
