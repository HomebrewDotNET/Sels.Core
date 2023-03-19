using Sels.Core.Cli.ArgumentParsing;

namespace Sels.Core.Cli.Templates.ArgumentParsing
{
    internal abstract class BaseOptionHandler<T, TArg> : BaseArgumentHandler<T, TArg>
    {
        // Properties
        public char OptionPrefix { get; }
        public char? Option { get; }
        public string LongOptionPrefix { get; }
        public string? LongOption { get; }
        public string FullOption => OptionPrefix.ToString() + Option;
        public string FullLongOption => LongOptionPrefix + LongOption;
        public bool IsOption => Option.HasValue;
        public bool IsLongOption => LongOption.HasValue();
        /// <inheritdoc/>
        public override string DisplayName { get; }

        public BaseOptionHandler(ArgumentParser<T> parser, char optionPrefix, string longOptionPrefix, char? option = null, string? longOption = null, Type? targetType = null) : base(parser, targetType)
        {
            if (option == null && longOption == null) throw new ArgumentException($"Either {nameof(option)} or {nameof(longOption)} needs to be provided");
            OptionPrefix = optionPrefix;
            LongOptionPrefix = longOptionPrefix.ValidateArgumentNotNullOrWhitespace(nameof(longOptionPrefix));
            if(option.HasValue) Option = option;
            if(longOption != null) LongOption = longOption.ValidateArgumentNotNullOrWhitespace(nameof(longOption));

            DisplayName = IsOption && IsLongOption ? $"Option {FullOption} | {FullLongOption}" : IsOption ? $"Option {FullOption}" : $"Option {FullLongOption}";
        }

        /// <inheritdoc/>
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
    }
}
