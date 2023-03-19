using Microsoft.Extensions.Logging;
using Sels.Core.Cli.ArgumentParsing;
using Sels.Core.Conversion.Converters;
using System.Linq.Expressions;
using System.Reflection;
using Sels.Core.Extensions.Reflection;

namespace Sels.Core.Cli.Templates.ArgumentParsing
{
    /// <summary>
    /// Template for creating new argument handlers.
    /// </summary>
    /// <typeparam name="T">The instance to add the arguments to</typeparam>
    /// <typeparam name="TArg">The type to parse to</typeparam>
    internal abstract class BaseArgumentHandler<T, TArg> : BaseArgumentHandler<T>, IArgumentHandlerBuilder<T, TArg>
    {
        // Fields
        private bool _doNotParseAsCollection;
        /// <summary>
        /// The actual type to convert to so it can be set.
        /// </summary>
        protected readonly Type _targetType;
        /// <summary>
        /// The delegate to split strings into multiple values so they can be used for parsing.
        /// </summary>
        protected Func<string, IEnumerable<string>>? _multiSplitter;
        /// <summary>
        /// Optional converter for converting the parsed values to <typeparamref name="TArg"/>.
        /// </summary>
        protected Func<object, TArg>? _converter;
        /// <summary>
        /// The types of the key/value pair if the argument needs to be parsed from key/value pairs.
        /// </summary>
        protected (string KeyName, Type Key, string ValueName, Type Value, string Format)? _keyValueInfo;
        /// <summary>
        /// The description for this argument.
        /// </summary>
        protected string _description;
        /// <summary>
        /// Optional delegate for overwriting the default parsing error that gets returning when conversion fails.
        /// </summary>
        protected Func<string, Type, bool, string>? _parsingErrorConstructor;
        /// <summary>
        /// A list of parsing conditions that must be passed before the current handler can parse arguments.
        /// </summary>
        protected readonly Lazy<List<Delegates.Condition<IParsedResult<T>>>> _parsingConditions = new Lazy<List<Delegates.Condition<IParsedResult<T>>>>();
        /// <summary>
        /// A list of validators that validates the parsed arguments.
        /// </summary>
        protected readonly Lazy<List<(Delegates.Condition<IParsedResult<T>, TArg> Validate, Func<IParsedResult<T>, TArg, string> CreateError)>> _validators = new Lazy<List<(Delegates.Condition<IParsedResult<T>, TArg> Validate, Func<IParsedResult<T>, TArg, string> CreateError)>>();

        // Properties
        /// <summary>
        /// If the command line argument values should be parsed to the element type instead of the target type.
        /// </summary>
        protected bool ParseToElementType => !_doNotParseAsCollection && _targetType.IsContainer() && !_targetType.IsString();
        /// <summary>
        /// If the command line argument values should be split up into key/value pairs.
        /// </summary>
        protected bool ParseAsKeyValuePairs => _keyValueInfo.HasValue;
        private Type KeyValuePairType => typeof(KeyValuePair<,>).MakeGenericType(_keyValueInfo.Value.Key, _keyValueInfo.Value.Value);

        /// <inheritdoc cref="BaseArgumentHandler{T, TArg}"/>.
        /// <param name="parser">The parser to delegate calls to</param>
        /// <param name="targetType">The actual target type to convert the values to</param>
        public BaseArgumentHandler(ArgumentParser<T> parser, Type? targetType = null) : base(parser)
        {
            _targetType = targetType ?? typeof(TArg);
        }

        #region Parsing
        /// <inheritdoc/>
        public override bool TryParse(string[] args, IResultBuilder builder, out object? parsed, out string[] modifiedArgs)
        {
            args.ValidateArgument(nameof(args));
            builder.ValidateArgument(nameof(builder));

            var result = TryParseArguments(args, builder, out var parsedArg, out modifiedArgs);
            parsed = parsedArg;
            return result;
        }
        /// <inheritdoc/>
        public override bool CanParse(IParsedResult<T> currentResult)
        {
            return !_parsingConditions.IsValueCreated || _parsingConditions.Value.All(x => x(currentResult));
        }
        /// <inheritdoc/>
        public override IEnumerable<string> Validate(IParsedResult<T> currentResult, object? parsed)
        {
            if (_validators.IsValueCreated)
            {
                var arg = parsed.Cast<TArg>();
                foreach (var validator in _validators.Value.Where(x => x.Validate(currentResult, arg)))
                {
                    yield return validator.CreateError(currentResult, arg);
                }
            }
        }
        #endregion

        #region Config
        /// <inheritdoc/>
        public virtual IArgumentHandlerBuilder<T, TArg> ConvertUsing(Func<object, TArg> converter)
        {
            _converter = converter.ValidateArgument(nameof(converter));
            return this;
        }
        /// <inheritdoc/>
        public virtual IArgumentHandlerBuilder<T, TArg> FromKeyValuePair<TKey, TValue>(string format = "{Key}={Value}", string keyName = "key", string valueName = "value")
        {
            format.ValidateArgumentNotNullOrWhitespace(nameof(format));
            format.ValidateArgument(x => !x.ContainsWhitespace(), $"{nameof(format)} cannot contain any whitespace characters");
            format.ValidateArgument(x => !x.Contains(CliConstants.ArgumentParsing.KeyValuePatternKeyName), $"{nameof(format)} does not contain {CliConstants.ArgumentParsing.KeyValuePatternKeyName}");
            format.ValidateArgument(x => !x.Contains(CliConstants.ArgumentParsing.KeyValuePatternValueName), $"{nameof(format)} does not contain {CliConstants.ArgumentParsing.KeyValuePatternValueName}");
            keyName.ValidateArgumentNotNullOrWhitespace(nameof(keyName));
            valueName.ValidateArgumentNotNullOrWhitespace(nameof(valueName));

            _keyValueInfo = (keyName, typeof(TKey), valueName, typeof(TValue), format);
            return this;
        }
        /// <inheritdoc/>
        public virtual IArgumentHandlerBuilder<T, TArg> FromMultiple(Func<string, IEnumerable<string>> splitter)
        {
            _multiSplitter = splitter.ValidateArgument(nameof(splitter));
            return this;
        }
        /// <inheritdoc/>
        public virtual IArgumentHandlerBuilder<T, TArg> FromMultiple(string splitter, bool trim = true)
        {
            splitter.ValidateArgumentNotNullOrWhitespace(nameof(splitter));

            _multiSplitter = x => x.Split(splitter, StringSplitOptions.RemoveEmptyEntries).Select(x => trim ? x.Trim() : x);

            return this;
        }
        /// <inheritdoc/>
        public virtual IArgumentHandlerBuilder<T, TArg> WithDescription(string description)
        {
            _description = description;
            return this;
        }
        /// <inheritdoc/>
        public IArgumentHandlerBuilder<T, TArg> WithDefault(TArg value) => WithDefault(() => value);
        /// <inheritdoc/>
        public IArgumentHandlerBuilder<T, TArg> ParseWhen(Delegates.Condition<IParsedResult<T>> condition)
        {
            condition.ValidateArgument(nameof(condition));

            _parsingConditions.Value.Add(condition);
            return this;
        }
        /// <inheritdoc/>
        public IArgumentHandlerBuilder<T, TArg> ValidIf(Delegates.Condition<IParsedResult<T>, TArg> condition, Func<IParsedResult<T>, TArg, string> errorConstructor)
        {
            condition.ValidateArgument(nameof(condition));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            _validators.Value.Add((condition, errorConstructor));
            return this;
        }
        /// <inheritdoc/>
        public IArgumentHandlerBuilder<T, TArg> WithParsingError(Func<string, Type, bool, string> errorConstructor)
        {
            _parsingErrorConstructor = errorConstructor.ValidateArgument(nameof(errorConstructor));
            return this;
        }
        /// <inheritdoc/>
        public IArgumentHandlerBuilder<T, TArg> DoNotParseAsCollection()
        {
            _doNotParseAsCollection = false;
            return this;
        }
        /// <inheritdoc/>
        public abstract IArgumentHandlerBuilder<T, TArg> WithDefault(Func<TArg> valueConstructor);
        #endregion

        #region Parsing Helpers
        /// <summary>
        /// Converts <paramref name="convertable"/> created from parsing the command line argument to the target type.
        /// </summary>
        /// <param name="convertable"></param>
        /// <returns></returns>
        protected TArg? ConvertToArgumentType(object? convertable)
        {
            using (_loggers.TraceMethod(this))
            {
                if (convertable == null) return default;

                return (_converter != null ? _converter(convertable) : _parser.Config.Converter.ConvertTo(convertable, _targetType)).Cast<TArg>();
            }
        }
        /// <summary>
        /// Creates an error message for <paramref name="value"/> who couldn't be converted to <paramref name="targetType"/>.
        /// </summary>
        /// <param name="value">The string value that couldn't be converted</param>
        /// <param name="targetType">The type <paramref name="value"/> couldn't be converted to</param>
        /// <param name="isKey">If <paramref name="value"/> came from a key/value pair this boolean indicates if it was the key that failed conversion</param>
        /// <returns>The error message</returns>
        /// <exception cref="InvalidOperationException"></exception>
        protected string CreateConversionError(string value, Type targetType, bool isKey = false)
        {
            using (_loggers.TraceMethod(this))
            {
                if (_parsingErrorConstructor != null)
                {
                    return _parsingErrorConstructor(value, targetType, isKey) ?? throw new InvalidOperationException($"Empty conversion error message returned");
                }
                else
                {
                    return isKey ? $"<{value}> is not a valid key for {DisplayName}" : $"<{value}> is not a valid value for {DisplayName}";
                }
            }
        }

        /// <summary>
        /// Parses multiple command line arguments to the handler target type.
        /// </summary>
        /// <param name="values">The values to parse</param>
        /// <param name="builder">Builder for modifying the parsing result</param>
        /// <returns>The object with the parsed values</returns>
        protected TArg? ParseValues(IResultBuilder builder, string[] values)
        {
            using(_loggers.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                values.ValidateArgumentNotNullOrEmpty(nameof(values));

                // Split values
                values = _multiSplitter != null ? values.SelectMany(x => _multiSplitter(x)).ToArray() : values;

                if (ParseAsKeyValuePairs)
                {
                    _loggers.Debug($"{DisplayName}: Multiple key/value pairs expected. Parsing to list of key value pairs with key type <{_keyValueInfo.Value.Key}> and value type <{_keyValueInfo.Value.Value}>");

                    // Parse each value to target type
                    List<object> parsed = new List<object>();
                    foreach (var value in values)
                    {
                        var converted = ParseToKeyValuePair(builder, value);
                        if (converted == null) continue;
                        parsed.Add(converted);
                    }

                    // Create typed list
                    var typedParsed = parsed.CreateList(KeyValuePairType);
                    // Convert parsed to expected argument type
                    return ConvertToArgumentType(typedParsed);
                }
                else
                {
                    // Get type to convert values to
                    Type elementType = _targetType;
                    if (ParseToElementType)
                    {
                        elementType = _targetType.GetElementTypeFromCollection();
                        _loggers.Debug($"{DisplayName}: Multiple values expected and target type is collection. Parsing to list of element type <{elementType}> from collection <{_targetType}>");
                    }
                    else
                    {
                        _loggers.Debug($"{DisplayName}: Multiple values expected. Parsing to list of element type {_targetType}");
                    }

                    // Parse each value to target type
                    List<object> parsed = new List<object>();
                    foreach(var value in values)
                    {
                        var converted = ParseTo(builder, value, elementType);
                        if (converted == null) continue;
                        parsed.Add(converted);
                    }

                    // Create typed list
                    var typedParsed = parsed.CreateList(elementType);
                    // Convert parsed to expected argument type
                    return ConvertToArgumentType(typedParsed);
                }
            }
        }
        /// <summary>
        /// Parses command line argument to the handler target type.
        /// </summary>
        /// <param name="value">THe value to parse</param>
        /// <param name="builder">Builder for modifying the parsing result</param>
        /// <returns>The object with the parsed values</returns>
        protected TArg? ParseValue(IResultBuilder builder, string value)
        {
            using (_loggers.TraceMethod(this))
            {
                builder.ValidateArgument(nameof(builder));
                value.ValidateArgument(nameof(value));

                if(_multiSplitter != null)
                {
                    return ParseValues(builder, value.AsArray());
                }
                else if (ParseAsKeyValuePairs)
                {
                    _loggers.Debug($"{DisplayName}: key/value pair expected. Parsing to key/value pair with key type <{_keyValueInfo.Value.Key}> and value type <{_keyValueInfo.Value.Value}>");
                    var parsed = ParseToKeyValuePair(builder, value);
                    return parsed != null ? ConvertToArgumentType(parsed) : default;
                }
                else
                {
                    _loggers.Debug($"{DisplayName}: Parsing to <{_targetType}>");
                    var parsed = ParseTo(builder, value, _targetType);
                    return parsed != null ? ConvertToArgumentType(parsed) : default;
                }
            }
        }

        private object? ParseTo(IResultBuilder builder, string value, Type targetType, bool isKey = false)
        {
            using (_loggers.TraceMethod(this))
            {
                _loggers.Debug($"Parsing <{value}> to type <{targetType}>");

                if(_parser.Config.Converter.TryConvertTo(value, targetType, out var converted))
                {
                    _loggers.Trace($"Converted <{value}> to <{converted}>");
                    return converted;
                }
                else
                {
                    _loggers.Warning($"Could not convert <{value}> to type <{targetType}>");
                    builder.AddError(CreateConversionError(value, targetType, isKey));
                    return null;
                }
            }
        }

        private object? ParseToKeyValuePair(IResultBuilder builder, string pair)
        {
            using (_loggers.TraceMethod(this))
            {
                _loggers.Debug($"Parsing <{pair}> to <{KeyValuePairType}>");

                // Extract key and value from pair string
                var format = _keyValueInfo.Value.Format;
                var key = format.ExtractFromFormat(CliConstants.ArgumentParsing.KeyValuePatternKeyName, pair, CliConstants.ArgumentParsing.KeyValuePatternValueName);
                if(key == null)
                {
                    builder.AddError($"Invalid format: Could not extract <{_keyValueInfo.Value.KeyName.ToLower()}> from <{pair}>");
                    _loggers.Warning($"Key could not be extracted from  pair <{pair}> using format <{format}>");
                    return null;
                }
                var value = format.ExtractFromFormat(CliConstants.ArgumentParsing.KeyValuePatternValueName, pair, CliConstants.ArgumentParsing.KeyValuePatternKeyName);
                if (value == null)
                {
                    builder.AddError($"Invalid format: Could not extract <{_keyValueInfo.Value.ValueName.ToLower()}> from <{pair}>");
                    _loggers.Warning($"Value could not be extracted from pair <{pair}> using format <{format}>");
                    return null;
                }

                // Convert string values to expected types
                var convertedKey = ParseTo(builder, key, _keyValueInfo.Value.Key, true);
                if (convertedKey == null) return null;
                var convertedValue = ParseTo(builder, value, _keyValueInfo.Value.Value);
                if(convertedValue == null) return null;

                // Convert to pair
                var keyValuePair = KeyValuePairType.Construct(convertedKey, convertedValue);
                _loggers.Trace($"Converted <{pair}> to <{keyValuePair}>");
                return keyValuePair;
            }
        }
        #endregion

        // Abstractions
        /// <inheritdoc cref="BaseArgumentHandler{T}.TryParse(string[], IResultBuilder, out object, out string[])"/>
        public abstract bool TryParseArguments(string[] args, IResultBuilder builder, out TArg? parsed, out string[] modifiedArgs);
    }
    /// <summary>
    /// Template for creating new argument handlers.
    /// </summary>
    /// <typeparam name="T">The instance to add the arguments to</typeparam>
    internal abstract class BaseArgumentHandler<T> : IArgumentParserBuilder<T>
    {
        // Fields
        /// <summary>
        /// The parser used to create this handler;
        /// </summary>
        protected readonly ArgumentParser<T> _parser;
        /// <summary>
        /// Optional loggers for tracing.
        /// </summary>
        protected readonly IEnumerable<ILogger>? _loggers;

        /// <inheritdoc cref="BaseArgumentHandler{T}"/>.
        /// <param name="parser">The parser to delegate calls to</param>
        public BaseArgumentHandler(ArgumentParser<T> parser)
        {
            _parser = parser.ValidateArgument(nameof(parser));
            _loggers = _parser.Config.Loggers;
        }

        #region Parser
        /// <inheritdoc/>
        public IArgumentHandlerSelector<T, TArg1> For<TArg1>(Expression<Func<T, TArg1>> property)
        {
            return _parser.For(property);
        }
        /// <inheritdoc/>
        public IArgumentHandlerSelector<T, object> For(PropertyInfo property)
        {
            return _parser.For(property);
        }
        /// <inheritdoc/>
        public IArgumentHandlerSelector<T, TArg1> SetValue<TArg1>(Action<T, TArg1> setter)
        {
            return _parser.SetValue<TArg1>(setter);
        }
        /// <inheritdoc/>
        public IArgumentHandlerSelector<T, object> For(PropertyInfo[] propertyHierarchy)
        {
            return _parser.For(propertyHierarchy);
        }
        /// <inheritdoc/>
        public IArgumentHandlerSelector<T, TArg> SetValue<TArg>(Action<TArg> setter)
        {
            return _parser.SetValue(setter);
        }
        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return DisplayName;
        }

        // Abstractions
        /// <summary>
        /// A human readable name of the current argument handler.
        /// </summary>
        public abstract string DisplayName { get; }
        /// <summary>
        /// Checks if the current handler can parse based on the current result <paramref name="currentResult"/>.
        /// </summary>
        /// <param name="currentResult">The current result during parsing</param>
        /// <returns>True if the current handler can parse arguments, otherwise false</returns>
        public abstract bool CanParse(IParsedResult<T> currentResult);
        /// <summary>
        /// Validates <paramref name="parsed"/> after being parsed.
        /// </summary>
        /// <param name="currentResult">The current result during parsing</param>
        /// <param name="parsed">The argument fully parsed by the current handler</param>
        /// <returns>An enumerator returning any validation errors, returns null or an empty enumerator when there are no validation errors</returns>
        public abstract IEnumerable<string> Validate(IParsedResult<T> currentResult, object? parsed);
        /// <summary>
        /// Parses any arguments in <paramref name="args"/>.
        /// </summary>
        /// <param name="args">List with all the arguments to parse</param>
        /// <param name="builder">A builder for modifying the <see cref="IParsedResult{T}"/></param>
        /// <param name="parsed">The object parsed from the arguments</param>
        /// <param name="modifiedArgs"><paramref name="args"/> with all the handled arguments removed</param>
        /// <returns>True if any arguments were handled, otherwise false</returns>
        public abstract bool TryParse(string[] args, IResultBuilder builder, out object? parsed, out string[] modifiedArgs);
    }
}
