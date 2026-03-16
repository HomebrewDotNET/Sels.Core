using Sels.Core.Cli.Contracts.ArgumentParsing;
using System.Linq.Expressions;
using System.Reflection;
using static Sels.Core.Delegates;

namespace Sels.Core.Cli.ArgumentParsing
{
    /// <summary>
    /// Service that parses command line arguments using delegates.
    /// </summary>
    public interface IArgumentParser : IArgumentParser<NullArguments> { }
    /// <summary>
    /// Service that parses command line arguments to an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to parse the arguments to</typeparam>
    public interface IArgumentParser<T> : IArgumentParserBuilder<T>
    {
        /// <summary>
        /// Parses command line arguments <paramref name="args"/> to an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="instance">The instance to parse to</param>
        /// <param name="args">The result of trying to parse <paramref name="args"/> to an instance of <typeparamref name="T"/></param>
        IParsedResult<T> Parse(T instance, string[] args);
        /// <summary>
        /// Executes <paramref name="action"/> after parsing.
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <returns>Current instance for method chaining</returns>
        IArgumentParser<T> OnParsed(Action<IParsedResult<T>> action);
        /// <summary>
        /// Executes <paramref name="action"/> after parsing.
        /// </summary>
        /// <param name="action">The action to execute. First arg is the parsed result, second arg is a builder to modify the result</param>
        /// <returns></returns>
        IArgumentParser<T> OnParsed(Action<IParsedResult<T>, IResultBuilder> action);
    }
    /// <summary>
    /// Configurator for configuring what arguments need to be parsed.
    /// </summary>
    /// <typeparam name="T">The type to parse the arguments to</typeparam>
    public interface IArgumentParserBuilder<T>
    {
        /// <summary>
        /// Creates a new argument handler for the property selected by <paramref name="property"/>. Can be a property selected from the properties on <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="TArg">The type the arguments will be parsed to</typeparam>
        /// <param name="property">The expression that selects the property on <typeparamref name="T"/> to parse to</param>
        /// <returns>A selector to configure what to parse from</returns>
        IArgumentHandlerSelector<T, TArg> For<TArg>(Expression<Func<T, TArg>> property);
        /// <summary>
        /// Created a new argument handler for property <paramref name="property"/> on <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property">The property to parse to</param>
        /// <returns>A selector to configure what to parse from</returns>
        IArgumentHandlerSelector<T, object> For(PropertyInfo property);
        /// <summary>
        /// Creates a new argument handler for the last property in <paramref name="propertyHierarchy"/>.
        /// </summary>
        /// <param name="propertyHierarchy">A list of properties where the first property is a property on <typeparamref name="T"/> and any following property is a property selected from the type of the previous</param>
        /// <returns>A selector to configure what to parse from</returns>
        IArgumentHandlerSelector<T, object> For(PropertyInfo[] propertyHierarchy);
        /// <summary>
        /// Creates a new argument handler where the value will be handler by <paramref name="setter"/>.
        /// </summary>
        /// <typeparam name="TArg">The type the arguments will be parsed to</typeparam>
        /// <param name="setter">The delegate that will handle the parsed value. First arg is the instance to parse to, second arg is the parsed argument</param>
        /// <returns>A selector to configure what to parse from</returns>
        IArgumentHandlerSelector<T, TArg> SetValue<TArg>(Action<T, TArg> setter);
        /// <summary>
        /// Creates a new argument handler where the value will be handler by <paramref name="setter"/>.
        /// </summary>
        /// <typeparam name="TArg">The type the arguments will be parsed to</typeparam>
        /// <param name="setter">The delegate that will handle the parsed value.</param>
        /// <returns>A selector to configure what to parse from</returns>
        IArgumentHandlerSelector<T, TArg> SetValue<TArg>(Action<TArg> setter);
    }
    /// <summary>
    /// Used to select what argument type to parse to <typeparamref name="TArg"/>.
    /// </summary>
    /// <typeparam name="T">The instance to add the arguments to</typeparam>
    /// <typeparam name="TArg">The type to parse to</typeparam>
    public interface IArgumentHandlerSelector<T, TArg>
    {
        /// <summary>
        /// Parses from a non-option in the beginning of the argument list.
        /// Example: mycmd (main_command) (sub_command) -q -f true -p Action=Build (argument) => (main_command) will be parsed using position 0, (sub_command) will be parsed using position 1.
        /// </summary>
        /// <param name="name">The name that is used in the help text</param>
        /// <returns>A configurator for the current argument handler</returns>
        IArgumentHandlerBuilder<T, TArg> FromCommand(string name);
        /// <summary>
        /// Parses from an option anywhere in the argument list.
        /// Example: mycmd -h => - is the short option prefix, h is the short option.
        /// </summary>
        /// <param name="shortOption">The short name of the option</param>
        /// <param name="configurator">Optional configurator for modifying how the option is parsed</param>
        /// <param name="valueType">Defines how the option can be used</param>
        /// <returns>A configurator for the current argument handler</returns>
        IArgumentHandlerBuilder<T, TArg> FromOption(char shortOption,  Action<IOptionHandlerBuilder>? configurator = null, OptionValueType valueType = OptionValueType.Default);
        /// <summary>
        /// Parses from an option anywhere in the argument list.
        /// Example: mycmd --help => -- is the long option prefix, help is the long option.
        /// </summary>
        /// <param name="longOption">The full name of the option</param>
        /// <param name="configurator">Optional configurator for modifying how the option is parsed</param>
        /// <param name="valueType">Defines how the option can be used</param>
        /// <returns>A configurator for the current argument handler</returns>
        IArgumentHandlerBuilder<T, TArg> FromOption(string longOption, Action<IOptionHandlerBuilder>? configurator = null, OptionValueType valueType = OptionValueType.Default);
        /// <summary>
        /// Parses from an option anywhere in the argument list.
        /// Example: mycmd -h --help => - is the short option prefix, h is the short option, -- is the long option prefix, help is the long option. Both examples mean the same.
        /// </summary>
        /// <param name="shortOption">The short name of the option</param>
        /// <param name="longOption">The full name of the option</param>
        /// <param name="configurator">Optional configurator for modifying how the option is parsed</param>
        /// <param name="valueType">Defines how the option can be used</param>
        /// <returns>A configurator for the current argument handler</returns>
        IArgumentHandlerBuilder<T, TArg> FromOption(char shortOption, string longOption, Action<IOptionHandlerBuilder>? configurator = null, OptionValueType valueType = OptionValueType.Default);
        /// <summary>
        /// Parses from a non-option at the end of the argument list.
        /// Example: mycmd (command) -q (source_file) (target_file) --overwrite => (source_file) will be parsed from position 0, (target_file) will be parsed from position 1.
        /// </summary>
        /// <param name="name">The name that is used in the help text</param>
        /// <returns>A configurator for the current argument handler</returns>
        IArgumentHandlerBuilder<T, TArg> FromArgument(string name);
        /// <summary>
        /// Parses from all non-options at the end of the argument list.
        /// Example: mycmd (command) (file_one) (file_two) (file_three) => (file_one), (file_two) and (file_three) will be parsed when (command) is configured using <see cref="FromCommand(string)"/>
        /// </summary>
        /// <param name="name">The name that is used in the help text</param>
        /// <returns>A configurator for the current argument handler</returns>
        IArgumentHandlerBuilder<T, TArg> FromArguments(string name);
        /// <summary>
        /// Parses from the stdin of the cli.
        /// </summary>
        /// <returns>A configurator for the current argument handler</returns>
        IArgumentHandlerBuilder<T, TArg> FromInput();
        /// <summary>
        /// Parses from an environment variable.
        /// </summary>
        /// <param name="name">The name of the environment to get the value from</param>
        /// <returns>A configurator for the current argument handler</returns>
        IArgumentHandlerBuilder<T, TArg> FromEnvironment(string name);
        /// <summary>
        /// Parses from anything configured using <paramref name="selector"/>.
        /// </summary>
        /// <param name="selector">The delegate to configure with</param>
        /// <returns>The parser to create additional configuration</returns>
        IArgumentParser<T> FromMultiple(Action<IArgumentHandlerSelector<T, TArg>> selector);
    }
    /// <summary>
    /// Configurator for configuring how arguments need to be parsed.
    /// </summary>
    /// <typeparam name="T">The instance to add the arguments to</typeparam>
    /// <typeparam name="TArg">The type to parse to</typeparam>
    public interface IArgumentHandlerBuilder<T, TArg> : IArgumentParserBuilder<T>
    {
        /// <summary>
        /// Gives a description to the argument when generating help documentation.
        /// </summary>
        /// <param name="description">A short decription describing what the argument is used for</param>
        /// <returns>Current instance for method chaining</returns>
        IArgumentHandlerBuilder<T, TArg> WithDescription(string description);
        /// <summary>
        /// Used to split single values into a list that will be converted to <typeparamref name="TArg"/>.
        /// </summary>
        /// <example>Given the following format: mycmd -c 1,2,3. If you want to parse that value to a list of ints you can do that by splitting 1,2,3 using <paramref name="splitter"/></example>
        /// <param name="splitter">The delegate used to split the single value into multiple values</param>
        /// <returns>Current instance for method chaining</returns>
        IArgumentHandlerBuilder<T, TArg> FromMultiple(Func<string, IEnumerable<string>> splitter);
        /// <summary>
        /// Used to split single values into a list that will be converted to <typeparamref name="TArg"/>.
        /// </summary>
        /// <example>Given the following format: mycmd -c 1,2,3. If you want to parse that value to a list of ints you can do that by splitting 1,2,3 using <paramref name="splitter"/></example>
        /// <param name="splitter">The delegate used to split the single value into multiple values</param>
        /// <param name="trim">Set to true to trim the split up values</param>
        /// <returns>Current instance for method chaining</returns>
        IArgumentHandlerBuilder<T, TArg> FromMultiple(string splitter, bool trim = true);
        /// <summary>
        /// Used to parse single values as key value pairs instead.
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <param name="keyName">Optional name for the key. Is used in help and error messages</param>
        /// <param name="valueName">Optional name for the value. Is used in help and error messages</param>
        /// <param name="format">Optional format how the key/value pairs are defined. Key will be extracted from {Key} and the value from {Value}. Cannot contain any whitespace</param>
        /// <returns>Current instance for method chaining</returns>
        IArgumentHandlerBuilder<T, TArg> FromKeyValuePair<TKey, TValue>(string format = "{Key}={Value}", string keyName = "key", string valueName = "value");
        /// <summary>
        /// Used to provide a default value when no arguments are supplied.
        /// </summary>
        /// <param name="valueConstructor">The delegate that returns the default value</param>
        /// <returns>Current instance for method chaining</returns>
        IArgumentHandlerBuilder<T, TArg> WithDefault(Func<TArg> valueConstructor);
        /// <summary>
        /// Used to provide a default value when no arguments are supplied.
        /// </summary>
        /// <param name="value">The default value to use</param>
        /// <returns>Current instance for method chaining</returns>
        IArgumentHandlerBuilder<T, TArg> WithDefault(TArg value);
        /// <summary>
        /// Used to overwrite the default error message returned when a string value couldn't be converted to a type.
        /// </summary>
        /// <param name="errorConstructor">Delegate that creates the error for the failed conversion. First arg is the string value that couldn't be converted, second arg is the target type of the conversion, third arg indicated if the string was the key in case of key value pairs</param>
        /// <returns></returns>
        IArgumentHandlerBuilder<T, TArg> WithParsingError(Func<string, Type, bool, string> errorConstructor);
        /// <summary>
        /// Used to convert the parsed values to <typeparamref name="TArg"/>.
        /// </summary>
        /// <param name="converter">The delegate used to convert the value. First arg is the object to convert, type will be different based on the configuration used</param>
        /// <returns>Current instance for method chaining</returns>
        IArgumentHandlerBuilder<T, TArg> ConvertUsing(Func<object, TArg> converter);
        /// <summary>
        /// Sets a condition when this argument is able to be parsed based on <paramref name="condition"/>. Can be used to only parse arguments when a certain command is provided. Is only used to determine when to create help documentation.
        /// </summary>
        /// <param name="condition">The delegate that dictates if this handler is allowed to parse. First arg is the current result, second arg are the command line argument strings</param>
        /// <returns>Current instance for method chaining</returns>
        IArgumentHandlerBuilder<T, TArg> ParseWhen(Condition<IParsedResult<T>> condition);
        /// <summary>
        /// Adds validation for the parsed argument.
        /// </summary>
        /// <param name="condition">The delegate that dictates if the parsed argument is valid</param>
        /// <param name="errorConstructor">The delegate that creates the error when the parsed argument isn't valid</param>
        /// <returns>Current instance for method chaining</returns>
        IArgumentHandlerBuilder<T, TArg> ValidIf(Condition<IParsedResult<T>, TArg> condition, Func<IParsedResult<T>, TArg, string> errorConstructor);
        /// <summary>
        /// By default the command line argument values will be converted to the elements type if <typeparamref name="TArg"/> is assignable to <see cref="IEnumerable{T}"/>. Calling this method disables that behaviour.
        /// </summary>
        /// <returns>Current instance for method chaining</returns>
        IArgumentHandlerBuilder<T, TArg> DoNotParseAsCollection();
    }
}
