namespace Sels.Core.Cli.ArgumentParsing
{
    /// <summary>
    /// Configurator for how an option can be parsed.
    /// </summary>
    public interface IOptionHandlerBuilder
    {
        /// <summary>
        /// Used to overwrite the default short option prefix.
        /// If you want an option to be parsed as /p you can set the short option flag to /.
        /// </summary>
        /// <param name="shortOptionPrefix">The prefix to use for the short option format</param>
        /// <returns>Current instance for method chaining</returns>
        IOptionHandlerBuilder WithPrefix(char shortOptionPrefix);
        /// <summary>
        /// Used to overwrite the default long option prefix.
        /// If you want an option to be parsed as -help you can set the long option flag to -.
        /// </summary>
        /// <param name="longOptionPrefix">The prefix to use for the long option format</param>
        /// <returns>Current instance for method chaining</returns>
        IOptionHandlerBuilder WithPrefix(string longOptionPrefix);
        /// <summary>
        /// Used to provide a custom format for options where an argument can be provided.
        /// If you want an option to be parsed as --action=Build you can set the format to {Option}={Arg}.
        /// </summary>
        /// <param name="pattern">The format to use. {Option} will be replaced by the prefix + option name and the argument from {Arg}. Cannot contain any whitespace</param>
        /// <returns>Current instance for method chaining</returns>
        IOptionHandlerBuilder FromFormat(string pattern);
        /// <summary>
        /// Allows an option to be defined multiple times.
        /// Calling this method will make following arguments valid: --action=Clean --action=Build --action=Publish.
        /// </summary>
        /// <returns>Current instance for method chaining</returns>
        IOptionHandlerBuilder AllowDuplicate();
        /// <summary>
        /// When no arguments can be provided to the option the value returned by <paramref name="valueConstructor"/> will be used to set the argument value. The default value is true.
        /// </summary>
        /// <param name="valueConstructor">The delegate that returns the value</param>
        /// <returns>Current instance for method chaining</returns>
        IOptionHandlerBuilder WhenDefined(Func<object> valueConstructor);
        /// <summary>
        /// When no arguments can be provided to the option <paramref name="value"/> will be used to set the argument value. The default value is true.
        /// </summary>
        /// <param name="value">The value to return</param>
        /// <returns>Current instance for method chaining</returns>
        IOptionHandlerBuilder WhenDefined(object value);
    }
}
