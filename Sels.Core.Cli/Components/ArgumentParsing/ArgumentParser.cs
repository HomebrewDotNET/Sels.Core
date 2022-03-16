using Sels.Core.Cli.Contracts.ArgumentParsing;
using Sels.Core.Cli.Templates.ArgumentParsing;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Conversion;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Sels.Core.Cli.ArgumentParsing.Handlers;
using Microsoft.Extensions.Logging;
using Sels.Core.Conversion.Converters;

namespace Sels.Core.Cli.ArgumentParsing
{
    /// <inheritdoc cref="IArgumentParser{T}"/>
    internal class ArgumentParser<T> : IArgumentParser<T>
    {
        // Fields
        private readonly IEnumerable<ILogger>? _loggers;
        private readonly Lazy<List<char>> _optionPrefixes = new Lazy<List<char>>();
        private readonly Lazy<List<string>> _longOptionPrefixes = new Lazy<List<string>>();
        private readonly Lazy<List<Action<IParsedResult<T>, IResultBuilder>>> _afterParseActions = new Lazy<List<Action<IParsedResult<T>, IResultBuilder>>>();
        private readonly Lazy<List<Action<T, object?>>> _afterSetActions = new Lazy<List<Action<T, object?>>>();

        // Properties
        /// <summary>
        /// The config for this argument parser.
        /// </summary>
        public ParserConfig Config { get; }
        /// <summary>
        /// The configured argument handlers.
        /// </summary>
        public List<(int Priority, BaseArgumentHandler<T> Handler, Action<T, object?> Setter)> Handlers { get; } = new List<(int Priority, BaseArgumentHandler<T> Handler, Action<T, object?> Setter)>();

        /// <inheritdoc cref="IArgumentParser{T}"/>
        /// <param name="builder">Optional builder for creating the argument handlers using the current instance</param>
        /// <param name="configurator">Optional delegate for configuring the argument parser</param>
        /// <param name="settings">Optional settings for the argument parser</param>
        public ArgumentParser(Action<IArgumentParserBuilder<T>>? builder = null, Action<IArgumentParserConfigurator>? configurator = null, ArgumentParserSettings settings = ArgumentParserSettings.CreateErrorsForUnparsed)
        {
            Config = new ParserConfig(configurator, settings);
            _loggers = Config.Loggers;
            if (builder != null) builder(this);
        }

        #region Config
        /// <inheritdoc/>
        public IArgumentHandlerSelector<T, TArg> For<TArg>(Expression<Func<T, TArg>> property)
        {
            property.ValidateArgument(nameof(property));
            var nestedProperties = property.ExtractProperties();
            
            return CreatePropertyHandler<TArg>(nestedProperties);
        }
        /// <inheritdoc/>
        public IArgumentHandlerSelector<T, object> For(PropertyInfo property)
        {
            property.ValidateArgument(nameof(property));

            return For(property.AsArray());
        }
        /// <inheritdoc/>
        public IArgumentHandlerSelector<T, object> For(PropertyInfo[] propertyHierarchy)
        {
            propertyHierarchy.ValidateArgumentNotNullOrEmpty(nameof(propertyHierarchy));

            return CreatePropertyHandler<object>(propertyHierarchy);
        }
        /// <inheritdoc/>
        public IArgumentHandlerSelector<T, TArg> SetValue<TArg>(Action<T, TArg> setter)
        {
            setter.ValidateArgument(nameof(setter));

            return SetValue(setter, typeof(TArg));
        }
        /// <inheritdoc/>
        public IArgumentHandlerSelector<T, TArg> SetValue<TArg>(Action<TArg> setter)
        {
            setter.ValidateArgument(nameof(setter));

            return SetValue<TArg>((instance, arg) => setter(arg));
        }
        /// <inheritdoc/>
        public IArgumentParser<T> OnParsed(Action<IParsedResult<T>> action)
        {
            action.ValidateArgument(nameof(action));

            return OnParsed((r, b) => action(r));
        }
        /// <inheritdoc/>
        public IArgumentParser<T> OnParsed(Action<IParsedResult<T>, IResultBuilder> action)
        {
            action.ValidateArgument(nameof(action));

            _afterParseActions.Value.Add(action);
            return this;
        }

        private IArgumentHandlerSelector<T, TArg> SetValue<TArg>(Action<T, TArg> setter, Type targetType)
        {
            setter.ValidateArgument(nameof(setter));
            targetType.ValidateArgument(nameof(targetType));

            return CreateSelector<TArg>((instance, arg) => setter(instance, arg.CastOrDefault<TArg>()), targetType);
        }

        private Selector<TArg> CreateSelector<TArg>(Action<T, object?> setter, Type? targetType = null)
        {
            return new Selector<TArg>(this, (instance, arg) =>
            {
                // Trigger actual setter
                setter(instance, arg);

                // Trigger any on set actions
                if (_afterSetActions.IsValueCreated) _afterSetActions.Value.Execute(a => a(instance, arg));
            }, targetType ?? typeof(TArg));
        }

        #region Property
        private IArgumentHandlerSelector<T, TArg> CreatePropertyHandler<TArg>(PropertyInfo[] propertyHierarchy)
        {
            propertyHierarchy.ValidateArgumentNotNullOrEmpty(nameof(propertyHierarchy));
            propertyHierarchy[0].PropertyType.ValidateArgumentAssignableTo("First property", typeof(T));

            return CreateSelector<TArg>(CreatePropertySetter(propertyHierarchy), propertyHierarchy.Last().PropertyType).OnSelected(x => x.ParseWhen(CreatePropertySetterCondition(propertyHierarchy)));
        }

        private Action<T, object?> CreatePropertySetter(PropertyInfo[] propertyHierarchy)
        {
            propertyHierarchy.ValidateArgumentNotNullOrEmpty(nameof(propertyHierarchy));

            return (instance, arg) =>
            {
                object currentInstance = instance;
                for(int i = 0; i < propertyHierarchy.Length; i++)
                {
                    var property = propertyHierarchy[i];

                    // If last property set otherwise get instance and continue loop
                    if(i == (propertyHierarchy.Length - 1))
                    {
                        property.SetValue(currentInstance, arg);
                    }
                    else
                    {
                        currentInstance = property.GetValue(currentInstance, null);
                    }
                }
            };
        }

        private Delegates.Condition<IParsedResult<T>> CreatePropertySetterCondition(PropertyInfo[] propertyHierarchy)
        {
            propertyHierarchy.ValidateArgumentNotNullOrEmpty(nameof(propertyHierarchy));

            return result =>
            {
                object currentInstance = result.Parsed;

                // Skip last property because that's the one that needs to be set
                foreach(var property in propertyHierarchy.SkipLast(1))
                {
                    if (currentInstance == null || !currentInstance.IsAssignableTo(property.ReflectedType)) return false;
                    currentInstance = property.GetValue(currentInstance);
                }

                return true;
            };
        }
        #endregion
        #endregion

        /// <inheritdoc/>
        public IParsedResult<T> Parse(T instance, string[] args)
        {
            using (_loggers.TraceMethod(this))
            {
                instance.ValidateArgument(nameof(instance));
                args.ValidateArgument(nameof(args));
                var result = new ParsedResult<T>(instance, args);

                using (_loggers.TraceAction($"Parsing <{args.Length}> command line arguments to <{instance}>"))
                {
                    if (args.HasValue())
                    {
                        bool hasParsed = false;

                        do
                        {
                            hasParsed = false;

                            foreach (var handler in Handlers.OrderBy(x => x.Priority))
                            {
                                var argumentHandler = handler.Handler;
                                // Check if handler can parse with the current result
                                if (argumentHandler.CanParse(result))
                                {
                                    using (_loggers.TraceAction(LogLevel.Debug, $"Parsing with handler <{argumentHandler.DisplayName}>"))
                                    {
                                        if(argumentHandler.TryParse(args, result, out var parsed, out var modifiedArgs))
                                        {
                                            hasParsed = true;
                                            _loggers.Debug($"Handler <{argumentHandler.DisplayName}> parsed <{parsed}>. Validating");
                                            var errors = argumentHandler.Validate(result, parsed);
                                            if (!errors.HasValue())
                                            {
                                                _loggers.Debug($"Value <{parsed}> parsed by handler <{argumentHandler.DisplayName}> is valid. Calling setter");
                                                handler.Setter(result.Parsed, parsed);
                                            }
                                            else
                                            {
                                                _loggers.Debug($"Value <{parsed}> parsed by handler <{argumentHandler.DisplayName}> was not valid and resulted in <{errors.Count()}> errors. Not setting value");
                                                errors.Execute(e => result.AddError(e));
                                            }
                                        }
                                        else
                                        {
                                            _loggers.Debug($"Handler <{argumentHandler.DisplayName}> could not parse anything");
                                        }

                                        // Remove handled arguments
                                        args = modifiedArgs.Where(x => x.HasValue()).ToArray();
                                    }
                                }
                                else
                                {
                                    _loggers.Debug($"Handler <{argumentHandler.DisplayName}> could not parse with current result. Skipping for now");
                                }
                            }

                        }
                        while (hasParsed);
                    }
                }

                // Set remaining args
                result.UnparsedArguments = args;

                // Create errors for unparsed if enabled
                if (Config.CreateErrorForUnhandled) result.UnparsedArguments.Execute(arg => result.AddError(IsOption(arg) ? $"Unknown option: {arg}" : $"Unknown argument: {arg}"));

                // Execute after parse actions
                if(_afterParseActions.IsValueCreated) _afterParseActions.Value.Execute(a => a(result, result));

                // Thrown exception when enabled
                if(Config.ThrowExceptionOnError && result.Errors.HasValue()) throw new InvalidCommandLineArgumentsException(result.Errors);

                return result;
            }
        }

        /// <summary>
        /// Checks if argument <paramref name="arg"/> is an option based on known option and long option prefixes.
        /// </summary>
        /// <param name="arg">The command line argument to check</param>
        /// <returns>True if <paramref name="arg"/> is an option, otherwise false</returns>
        internal bool IsOption(string arg)
        {
            if (!arg.HasValue()) return false;
            if (_optionPrefixes.IsValueCreated && _optionPrefixes.Value.Any(x => arg.StartsWith(x))) return true;
            if (_longOptionPrefixes.IsValueCreated && _longOptionPrefixes.Value.Any(x => arg.StartsWith(x))) return true;

            return arg.StartsWith(Config.OptionPrefix) || arg.StartsWith(Config.LongOptionPrefix);
        }
        /// <summary>
        /// Adds an option prefix that gets checked by <see cref="IsOption(string)"/>.
        /// </summary>
        /// <param name="prefix">The short option prefix to add</param>
        internal void AddOptionPrefix(char prefix)
        {
            if(!_optionPrefixes.Value.Contains(prefix)) _optionPrefixes.Value.Add(prefix);
        }
        /// <summary>
        /// Adds a long option prefix that gets checked by <see cref="IsOption(string)"/>.
        /// </summary>
        /// <param name="prefix">The long option prefix to add</param>
        internal void AddLongOptionPrefix(string prefix)
        {
            prefix.ValidateArgumentNotNullOrWhitespace(nameof(prefix));

            if (!_longOptionPrefixes.Value.Contains(prefix)) _longOptionPrefixes.Value.Add(prefix);
        }
        internal ArgumentParser<T> OnSet(Action<T, object?> onSetAction)
        {
            onSetAction.ValidateArgument(nameof(onSetAction));

            _afterSetActions.Value.Add(onSetAction);
            return this;
        }

        #region Classes
        private class Selector<TArg> : IArgumentHandlerSelector<T, TArg>
        {
            // Fields
            private readonly ArgumentParser<T> _parser;
            private readonly Type _targetType;
            private readonly IEnumerable<ILogger>? _loggers;
            private readonly Action<T, object?> _setter;
            private readonly List<Action<IArgumentHandlerBuilder<T, TArg>>> _selectedActions = new List<Action<IArgumentHandlerBuilder<T, TArg>>>();

            public Selector(ArgumentParser<T> parser, Action<T, object?> setter, Type targetType)
            {
                _parser = parser.ValidateArgument(nameof(parser));
                _targetType = targetType.ValidateArgument(nameof(targetType));
                _setter = setter.ValidateArgument(nameof(setter));
                _loggers = parser.Config.Loggers;
            }

            /// <inheritdoc/>
            public IArgumentHandlerBuilder<T, TArg> FromArgument(string name)
            {
                name.ValidateArgumentNotNullOrWhitespace(nameof(name));

                throw new NotImplementedException();
            }
            /// <inheritdoc/>
            public IArgumentHandlerBuilder<T, TArg> FromArguments(string name)
            {
                throw new NotImplementedException();
            }
            /// <inheritdoc/>
            public IArgumentHandlerBuilder<T, TArg> FromCommand(string name)
            {
                throw new NotImplementedException();
            }
            /// <inheritdoc/>
            public IArgumentHandlerBuilder<T, TArg> FromOption(char shortOption, Action<IOptionHandlerBuilder>? configurator = null, OptionValueType valueType = OptionValueType.Default)
            {
                _loggers.Log($"Creating handler for option {shortOption}");
                var handler = CreateOptionHandler(shortOption, null, configurator, valueType, out var position);
                _parser.Handlers.Add((position, handler, _setter));
                return TriggerActions(handler);
            }
            /// <inheritdoc/>
            public IArgumentHandlerBuilder<T, TArg> FromOption(string longOption, Action<IOptionHandlerBuilder>? configurator = null, OptionValueType valueType = OptionValueType.Default)
            {
                longOption.ValidateArgumentNotNullOrWhitespace(nameof(longOption));

                _loggers.Log($"Creating handler for long option {longOption}");
                var handler = CreateOptionHandler(null, longOption, configurator, valueType, out var position);
                _parser.Handlers.Add((position, handler, _setter));
                return TriggerActions(handler);
            }
            /// <inheritdoc/>
            public IArgumentHandlerBuilder<T, TArg> FromOption(char shortOption, string longOption, Action<IOptionHandlerBuilder>? configurator = null, OptionValueType valueType = OptionValueType.Default)
            {
                longOption.ValidateArgumentNotNullOrWhitespace(nameof(longOption));

                _loggers.Log($"Creating handler for long option {longOption}");
                var handler = CreateOptionHandler(shortOption, longOption, configurator, valueType, out var position);
                _parser.Handlers.Add((position, handler, _setter));
                return TriggerActions(handler);
            }
            /// <inheritdoc/>
            public IArgumentHandlerBuilder<T, TArg> FromEnvironment(string name)
            {
                throw new NotImplementedException();
            }
            /// <inheritdoc/>
            public IArgumentHandlerBuilder<T, TArg> FromInput()
            {
                throw new NotImplementedException();
            }
            /// <inheritdoc/>
            public IArgumentParser<T> FromMultiple(Action<IArgumentHandlerSelector<T, TArg>> selector)
            {
                selector.ValidateArgument(nameof(selector));

                selector(this);

                return _parser;
            }

            /// <summary>
            /// Executes <paramref name="onSelectedAction"/> when a handler is selected using the current selector.
            /// </summary>
            /// <param name="onSelectedAction">The action to execute</param>
            /// <returns>Current selector for method chaining</returns>
            public Selector<TArg> OnSelected(Action<IArgumentHandlerBuilder<T, TArg>> onSelectedAction)
            {
                onSelectedAction.ValidateArgument(nameof(onSelectedAction));

                _selectedActions.Add(onSelectedAction);
                return this;
            }

            private BaseArgumentHandler<T, TArg> CreateOptionHandler(char? option, string? longOption, Action<IOptionHandlerBuilder>? configurator, OptionValueType valueType, out int position)
            {
                using (_loggers.TraceMethod(this))
                {
                    position = 1;
                    if (!option.HasValue && longOption == null) throw new ArgumentException($"Either {nameof(option)} or {nameof(longOption)} needs to be provided");
                    var handlerConfig = new OptionHandlerBuilder(configurator);
                    if (handlerConfig.OptionPrefix.HasValue) _parser.AddOptionPrefix(handlerConfig.OptionPrefix.Value);
                    if (handlerConfig.LongOptionPrefix.HasValue()) _parser.AddLongOptionPrefix(handlerConfig.LongOptionPrefix);

                    if (valueType == OptionValueType.Default)
                    {
                        _loggers.Debug($"Option type is {OptionValueType.Default}. Setting actual option type based on <{_targetType}>");
                        if (_targetType.IsAssignableTo<bool>())
                        {
                            _loggers.Debug($"Type is <{typeof(bool)}>. Setting option type to <{OptionValueType.None}>");
                            valueType = OptionValueType.None;
                        }
                        else if (_targetType.IsAssignableTo<IEnumerable>() && !_targetType.IsString())
                        {
                            _loggers.Debug($"Type is <{typeof(IEnumerable)}>. Setting option type to <{OptionValueType.List}>");
                            valueType = OptionValueType.List;
                            position = 3;
                        }
                        else
                        {
                            _loggers.Debug($"Setting option type to default <{OptionValueType.Single}>");
                            valueType = OptionValueType.Single;
                            position = 2;
                        }
                    }

                    switch (valueType)
                    {
                        case OptionValueType.None:
                            if (handlerConfig.DuplicateAllowed == true) throw new InvalidArgumentParserConfiguration($"Duplicate allowed is not compatible with flag options");
                            if (handlerConfig.Format.HasValue()) throw new InvalidArgumentParserConfiguration($"Format is not valid for flag options");
                            return new FlagHandler<T, TArg>(_parser, handlerConfig.OptionPrefix ?? _parser.Config.OptionPrefix, handlerConfig.LongOptionPrefix ?? _parser.Config.LongOptionPrefix, handlerConfig.DefinedValueConstructor != null ? handlerConfig.DefinedValueConstructor() : true, option, longOption);
                        case OptionValueType.List:
                            throw new NotImplementedException();
                            break;
                        case OptionValueType.Single:
                            if (handlerConfig.DefinedValueConstructor != null) throw new InvalidArgumentParserConfiguration($"Defined value is not valid with value options");
                            return new SingleOptionHandler<T, TArg>(handlerConfig.Format, handlerConfig.DuplicateAllowed, _parser, handlerConfig.OptionPrefix ?? _parser.Config.OptionPrefix, handlerConfig.LongOptionPrefix ?? _parser.Config.LongOptionPrefix, option, longOption);
                        default: throw new NotSupportedException($"Option value type <{valueType}> is not supported");
                    }
                }
            }

            private IArgumentHandlerBuilder<T, TArg> TriggerActions(IArgumentHandlerBuilder<T, TArg> builder)
            {
                _selectedActions.Execute(x => x(builder));
                return builder;
            }
        }

        internal class ParserConfig : IArgumentParserConfigurator
        {
            /// <summary>
            /// The default prefix for options.
            /// </summary>
            public char OptionPrefix { get; private set; } = '-';
            /// <summary>
            /// The default prefix for long options.
            /// </summary>
            public string LongOptionPrefix { get; private set; } = "--";
            /// <summary>
            /// Optional loggers for debugging.
            /// </summary>
            public IEnumerable<ILogger>? Loggers { get; private set; }
            /// <summary>
            /// The default converter to use.
            /// </summary>
            public ITypeConverter Converter { get; private set; } = GenericConverter.DefaultConverter;
            /// <summary>
            /// If errors should be created for unparsed command line arguments.
            /// </summary>
            public bool CreateErrorForUnhandled { get; }
            /// <summary>
            /// If an exception should be thrown when parsing errors occured.
            /// </summary>
            public bool ThrowExceptionOnError { get; set; }

            public ParserConfig(Action<IArgumentParserConfigurator>? configurator = null, ArgumentParserSettings settings = ArgumentParserSettings.CreateErrorsForUnparsed)
            {
                if (configurator != null) configurator(this);
                CreateErrorForUnhandled = settings.HasFlag(ArgumentParserSettings.CreateErrorsForUnparsed);
                ThrowExceptionOnError = settings.HasFlag(ArgumentParserSettings.ThrowExceptionOnError);
            }

            public IArgumentParserConfigurator WithConverter(ITypeConverter converter)
            {
                Converter = converter.ValidateArgument(nameof(converter));
                return this;
            }

            public IArgumentParserConfigurator WithLoggers(IEnumerable<ILogger>? loggers)
            {
                Loggers = loggers;
                return this;
            }

            public IArgumentParserConfigurator WithLongOptionPrefix(string prefix)
            {
                LongOptionPrefix = prefix.ValidateArgumentNotNullOrWhitespace(nameof(prefix));
                return this;
            }

            public IArgumentParserConfigurator WithOptionPrefix(char prefix)
            {
                OptionPrefix = prefix;
                return this;
            }
        }
        #endregion
    }

    /// <summary>
    /// Exposes extra configuration for an argument parser.
    /// </summary>
    public interface IArgumentParserConfigurator
    {
        /// <summary>
        /// Overwrites the default prefix used for options. (e.g., -f and -a)
        /// </summary>
        /// <param name="prefix">The prefix to use</param>
        /// <returns>Current configurator for method chaining</returns>
        IArgumentParserConfigurator WithOptionPrefix(char prefix);
        /// <summary>
        /// Overwrites the default prefix used for long options. (e.g., --force and --all)
        /// </summary>
        /// <param name="prefix">The prefix to use</param>
        /// <returns>Current configurator for method chaining</returns>
        IArgumentParserConfigurator WithLongOptionPrefix(string prefix);
        /// <summary>
        /// Adds loggers for debugging the parser.
        /// </summary>
        /// <param name="loggers">Enumerator that returns the loggers to use</param>
        /// <returns>Current configurator for method chaining</returns>
        IArgumentParserConfigurator WithLoggers(IEnumerable<ILogger>? loggers);
        /// <summary>
        /// Overwrites the default converter that converts the command line strings to the required types.
        /// </summary>
        /// <param name="converter">The converter to use</param>
        /// <returns>Current configurator for method chaining</returns>
        IArgumentParserConfigurator WithConverter(ITypeConverter converter);
    }
    /// <summary>
    /// Exposes extra settings for an argument parser.
    /// </summary>
    [Flags]
    public enum ArgumentParserSettings
    {
        /// <summary>
        /// No settings selected.
        /// </summary>
        None = 0,
        /// <summary>
        /// If errors should be created when some command line arguments could not be parsed.
        /// </summary>
        CreateErrorsForUnparsed = 1,
        /// <summary>
        /// If the parser should throw an <see cref="InvalidCommandLineArgumentsException"/> when any errors happened during parsing.
        /// </summary>
        ThrowExceptionOnError = 2
    }
}
