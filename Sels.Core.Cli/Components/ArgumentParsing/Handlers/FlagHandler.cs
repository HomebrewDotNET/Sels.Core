using Microsoft.Extensions.Logging;
using Sels.Core.Cli.Templates.ArgumentParsing;
using Sels.Core.Extensions.Text;

namespace Sels.Core.Cli.ArgumentParsing.Handlers
{
    /// <summary>
    /// Parses options without any arguments
    /// </summary>
    internal class FlagHandler<T, TArg> : BaseOptionHandler<T, TArg>
    {
        // Fields
        private object _definedValue;

        /// <inheritdoc cref="FlagHandler{T, TArg}"/>.
        /// <param name="parser">The parser using to create this handler</param>
        /// <param name="optionPrefix">The prefix to use for the short option</param>
        /// <param name="longOptionPrefix">The prefix to use for the full option</param>
        /// <param name="definedValue">The value to return when the configured flag is defined</param>
        /// <param name="option">The option name</param>
        /// <param name="longOption">The full option name</param>
        public FlagHandler(ArgumentParser<T> parser, char optionPrefix, string longOptionPrefix, object definedValue, char? option, string? longOption) :base(parser, optionPrefix, longOptionPrefix, option, longOption)
        {
            _definedValue = definedValue;
        }

        /// <inheritdoc />
        public override bool TryParseArguments(string[] args, IResultBuilder builder, out TArg? parsed, out string[] modifiedArgs)
        {
            using (_loggers.TraceMethod(this))
            {
                var anyError = false;
                var anyParsed = false;
                parsed = default;
                modifiedArgs = args;

                _loggers.Log($"{DisplayName} parsing <{args.Length}> arguments");

                GetArguments(args, out var optionIndexes, out var longOptionIndexes);

                if (optionIndexes.HasValue() && optionIndexes.Length > 1)
                {
                    var message = $"Flag {FullOption} found multiple times in: {optionIndexes.Select(i => $"\"{args[i]}\"").JoinString()}";
                    _loggers.Warning(message);
                    builder.AddError(message);
                    anyError = true;
                }

                if (longOptionIndexes.HasValue() && optionIndexes.Length > 1)
                {
                    var message = $"Flag {FullLongOption} found multiple times in: {longOptionIndexes.Select(i => $"\"{args[i]}\"").JoinString()}";
                    _loggers.Warning(message);
                    anyError = true;
                    builder.AddError(message);
                }

                if(anyError) return false;

                if (optionIndexes.Length == 1)
                {
                    _loggers.Debug($"{DisplayName} parsing <{FullOption}>");
                    // Remove option from argument
                    var index = optionIndexes[0];
                    var newValue = modifiedArgs[index].Replace(Option.ToString(), string.Empty);

                    // Remove from argument list when just the prefix is left
                    modifiedArgs[index] = newValue.Equals(OptionPrefix.ToString()) ? string.Empty : newValue;

                    // Set parsed
                    builder.AddParsedOption(FullOption);

                    anyParsed = true;
                }

                if (longOptionIndexes.Length == 1)
                {
                    _loggers.Debug($"{DisplayName} parsing <{FullLongOption}>");
                    // Remove fron argument list
                    args[optionIndexes[0]] = string.Empty;

                    // Set parsed
                    builder.AddParsedLongOption(FullLongOption);
                    anyParsed = true;
                }

                if(anyParsed) parsed = ConvertToArgumentType(_definedValue);

                return anyParsed;
            }
        }

        /// <inheritdoc />
        public override IArgumentHandlerBuilder<T, TArg> WithDefault(Func<TArg> valueConstructor)
        {
            using (_loggers.TraceMethod(this))
            {
                valueConstructor.ValidateArgument(nameof(valueConstructor));

                _parser.OnParsed(x =>
                {
                    if ((IsOption && !x.ParsedOptions.Any(x => x.Key.Equals(FullOption))) || (IsLongOption && !x.ParsedLongOptions.Any(x => x.Key.Equals(FullLongOption))))
                    {
                        _loggers.Debug($"{DisplayName} was not in the argument list. Setting default value");
                        var value = valueConstructor();
                        _parser.Handlers.First(x => x.Handler.Equals(this)).Setter(x.Parsed, value);
                    }
                });

                return this;
            }
        }
        /// <inheritdoc />
        public override IArgumentHandlerBuilder<T, TArg> FromKeyValuePair<TKey, TValue>(string pattern = "{Key}={Value}", string keyName = "key", string valueName = "value")
        {
            throw new InvalidArgumentParserConfiguration($"Key value pairs are not compatible with flags");
        }
        /// <inheritdoc />
        public override IArgumentHandlerBuilder<T, TArg> FromMultiple(Func<string, IEnumerable<string>> splitter)
        {
            throw new InvalidArgumentParserConfiguration($"Multiple values are not compatible with flags");
        }
        /// <inheritdoc />
        public override IArgumentHandlerBuilder<T, TArg> FromMultiple(string splitter, bool trim = true)
        {
            throw new InvalidArgumentParserConfiguration($"Multiple values are not compatible with flags");
        }
        /// <inheritdoc />
        public override string ToString()
        {
            return IsOption && IsLongOption ? $"Flag {FullOption} | {FullLongOption}" : IsOption ? $"Flag {FullOption}" : $"Flag {FullLongOption}"; ;
        }

        private void GetArguments(string[] args, out int[] optionIndexes, out int[] longOptionIndexes)
        {
            List<int> tempOptionIndexes = new();
            List<int> tempLongOptionIndexes = new();

            var longOptionPrefixContainsOptionPrefix = LongOptionPrefix.Contains(OptionPrefix);

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (IsOption && arg.StartsWith(OptionPrefix) && arg.Contains(Option.Value) && (!longOptionPrefixContainsOptionPrefix || !arg.StartsWith(LongOptionPrefix)))
                {
                    tempOptionIndexes.Add(i);
                }

                if (IsLongOption && arg.Equals(FullLongOption))
                {
                    tempLongOptionIndexes.Add(i);
                }
            }

            optionIndexes = tempOptionIndexes.ToArray();
            longOptionIndexes = tempLongOptionIndexes.ToArray();
        }
    }
}
