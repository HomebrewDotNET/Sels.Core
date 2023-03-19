namespace Sels.Core.Cli.ArgumentParsing
{
    /// <inheritdoc cref="IParsedResult{T}"/>
    internal class ParsedResult<T> : IParsedResult<T>, IResultBuilder
    {
        // Fields
        private readonly Dictionary<int, string> _parsedCommands = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _parsedArguments = new Dictionary<int, string>();
        private readonly Dictionary<string, List<string>> _parsedOptions = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, List<string>> _parsedLongOptions = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, string> _parsedEnvironmentVariables = new Dictionary<string, string>();
        private readonly List<string> _errors = new List<string>();

        // Properties
        /// <inheritdoc />
        public T Parsed { get; }
        /// <inheritdoc />
        public string[] Arguments { get; }
        /// <inheritdoc />
        public string? ParsedInput { get; private set; }
        /// <inheritdoc />
        public IReadOnlyDictionary<int, string> ParsedCommands => _parsedCommands;
        /// <inheritdoc />
        public IReadOnlyDictionary<int, string> ParsedArguments => _parsedArguments;
        /// <inheritdoc />
        public IReadOnlyDictionary<string, string[]> ParsedOptions => _parsedOptions.ToDictionary(x => x.Key, x => x.Value.ToArray());
        /// <inheritdoc />
        public IReadOnlyDictionary<string, string[]> ParsedLongOptions => _parsedLongOptions.ToDictionary(x => x.Key, x => x.Value.ToArray());
        /// <inheritdoc />
        public IReadOnlyDictionary<string, string> ParsedEnvironmentVariables => _parsedEnvironmentVariables;
        /// <inheritdoc />
        public string[] Errors => _errors.ToArray();
        /// <inheritdoc />
        public string[] UnparsedArguments { get; internal set; }
        /// <inheritdoc cref="ParsedResult{T}"/>.
        /// <param name="instance">The instance to parse to</param>
        /// <param name="args">The command line arguments to parse from</param>
        public ParsedResult(T instance, string[] args)
        {
            Parsed = instance.ValidateArgument(nameof(instance));
            Arguments = args.ValidateArgumentNotNullOrEmpty(nameof(args));
        }

        #region Builder
        /// <inheritdoc />
        public IResultBuilder AddError(string message)
        {
            message.ValidateArgumentNotNullOrWhitespace(nameof(message));

            _errors.Add(message);
            return this;
        }
        /// <inheritdoc />
        public IResultBuilder AddParsedArgument(int position, string argument)
        {
            position.ValidateArgumentLargerOrEqual(nameof(position), 0);
            argument.ValidateArgumentNotNullOrWhitespace(nameof(argument));

            _parsedArguments.Add(position, argument);
            return this;
        }
        /// <inheritdoc />
        public IResultBuilder AddParsedCommand(int position, string value)
        {
            position.ValidateArgumentLargerOrEqual(nameof(position), 0);
            value.ValidateArgumentNotNullOrWhitespace(nameof(value));

            _parsedCommands.Add(position, value);
            return this;
        }
        /// <inheritdoc />
        public IResultBuilder AddParsedEnvironmentVariable(string name, string value)
        {
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));
            value.ValidateArgumentNotNullOrWhitespace(nameof(value));

            _parsedEnvironmentVariables.Add(name, value);
            return this;
        }
        /// <inheritdoc />
        public IResultBuilder AddParsedInput(string input)
        {
            input.ValidateArgumentNotNullOrWhitespace(nameof(input));

            ParsedInput = input;
            return this;
        }
        /// <inheritdoc />
        public IResultBuilder AddParsedLongOption(string option, string? value = null)
        {
            option.ValidateArgumentNotNullOrWhitespace(nameof(option));

            if (value.HasValue())
            {
                _parsedLongOptions.AddValueToList(option, value);
            }
            else if(!_parsedLongOptions.ContainsKey(option))
            {
                _parsedLongOptions.Add(option, new List<string>());
            }

            return this;
        }
        /// <inheritdoc />
        public IResultBuilder AddParsedLongOption(string option, IEnumerable<string> values)
        {
            option.ValidateArgumentNotNullOrWhitespace(nameof(option));
            values.ValidateArgumentNotNullOrEmpty(nameof(values));

            _parsedLongOptions.AddValues(option, values);

            return this;
        }
        /// <inheritdoc />
        public IResultBuilder AddParsedOption(string option, string? value = null)
        {
            option.ValidateArgumentNotNullOrWhitespace(nameof(option));

            if (value.HasValue())
            {
                _parsedOptions.AddValueToList(option, value);
            }
            else if (!_parsedLongOptions.ContainsKey(option))
            {
                _parsedOptions.Add(option, new List<string>());
            }

            return this;
        }
        /// <inheritdoc />
        public IResultBuilder AddParsedOption(string option, IEnumerable<string> values)
        {
            option.ValidateArgumentNotNullOrWhitespace(nameof(option));
            values.ValidateArgumentNotNullOrEmpty(nameof(values));

            _parsedOptions.AddValues(option, values);

            return this;
        }
        #endregion
    }
}
