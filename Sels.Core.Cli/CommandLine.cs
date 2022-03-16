using Sels.Core.Cli.ArgumentParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Cli
{
    /// <summary>
    /// Helper methods for running code from an cli.
    /// </summary>
    public static class CommandLine
    {
        /// <summary>
        /// The default exit code that gets returned when code was executed without any exceptions.
        /// </summary>
        public static int SuccesfulExitCode { get; set; } = 0;
        /// <summary>
        /// The exit code that gets returned when an exception that isn't <see cref="CommandLineException"/> gets thrown.
        /// </summary>
        public static int ErrorExitCode { get; set; } = 1;
        /// <summary>
        /// Helper methods for parsing command line arguments.
        /// </summary>
        public class Argument
        {
            /// <summary>
            /// Parses <paramref name="args"/> to a new instance of <typeparamref name="T"/>.
            /// </summary>
            /// <typeparam name="T">The type to parse to</typeparam>
            /// <param name="args">The command line arguments to parse</param>
            /// <param name="configurator">Optional delegate for configuring the argument parser</param>
            /// <param name="settings">Optional settings for the argument parser</param>
            /// <returns>The result from parsing <paramref name="args"/></returns>
            public static IParsedResult<T> Parse<T>(string[] args, Action<IArgumentParserConfigurator>? configurator = null, ArgumentParserSettings settings = ArgumentParserSettings.CreateErrorsForUnparsed) where T : new()
            {
                args.ValidateArgument(nameof(args));

                return Parse<T>(new T(), args, configurator, settings);
            }
            /// <summary>
            /// Parses <paramref name="args"/> to <paramref name="instance"/>.
            /// </summary>
            /// <typeparam name="T">The type to parse to</typeparam>
            /// <param name="instance">The instance to parse to</param>
            /// <param name="args">The command line arguments to parse</param>
            /// <param name="configurator">Optional delegate for configuring the argument parser</param>
            /// <param name="settings">Optional settings for the argument parser</param>
            /// <returns>The result from parsing <paramref name="args"/></returns>
            public static IParsedResult<T> Parse<T>(T instance, string[] args, Action<IArgumentParserConfigurator>? configurator = null, ArgumentParserSettings settings = ArgumentParserSettings.CreateErrorsForUnparsed)
            {
                args.ValidateArgument(nameof(args));
                instance.ValidateArgument(nameof(instance));

                return Parse<T>(x => { }, instance, args, configurator, settings);
            }
            /// <summary>
            /// Parses <paramref name="args"/> to a new instance of <typeparamref name="T"/>.
            /// </summary>
            /// <typeparam name="T">The type to parse to</typeparam>
            /// <param name="builder">Builder to configure how to parse <typeparamref name="T"/></param>
            /// <param name="args">The command line arguments to parse</param>
            /// <param name="configurator">Optional delegate for configuring the argument parser</param>
            /// <param name="settings">Optional settings for the argument parser</param>
            /// <returns>The result from parsing <paramref name="args"/></returns>
            public static IParsedResult<T> Parse<T>(Action<IArgumentParserBuilder<T>> builder, string[] args, Action<IArgumentParserConfigurator>? configurator = null, ArgumentParserSettings settings = ArgumentParserSettings.CreateErrorsForUnparsed) where T : new()
            {
                args.ValidateArgument(nameof(args));
                builder.ValidateArgument(nameof(builder));

                return Parse<T>(builder, new T(), args, configurator, settings);
            }
            /// <summary>
            /// Parses <paramref name="args"/> to <paramref name="instance"/>.
            /// </summary>
            /// <typeparam name="T">The type to parse to</typeparam>
            /// <param name="builder">Builder to configure how to parse <typeparamref name="T"/></param>
            /// <param name="instance">The instance to parse to</param>
            /// <param name="args">The command line arguments to parse</param>
            /// <param name="configurator">Optional delegate for configuring the argument parser</param>
            /// <param name="settings">Optional settings for the argument parser</param>
            /// <returns>The result from parsing <paramref name="args"/></returns>
            public static IParsedResult<T> Parse<T>(Action<IArgumentParserBuilder<T>> builder, T instance, string[] args, Action<IArgumentParserConfigurator>? configurator = null, ArgumentParserSettings settings = ArgumentParserSettings.CreateErrorsForUnparsed)
            {
                args.ValidateArgument(nameof(args));
                instance.ValidateArgument(nameof(instance));
                builder.ValidateArgument(nameof(builder));

                return new ArgumentParser<T>(builder).Parse(instance, args);
            }
            /// <summary>
            /// Parses <paramref name="args"/> where the parsed arguments are handled by using <see cref="IArgumentParserBuilder{T}.SetValue{TArg}(Action{TArg})"/> method.
            /// </summary>
            /// <param name="builder">Builder to configure how to parse</param>
            /// <param name="args">The command line arguments to parse</param>
            /// <param name="configurator">Optional delegate for configuring the argument parser</param>
            /// <param name="settings">Optional settings for the argument parser</param>
            /// <returns>The result from parsing <paramref name="args"/></returns>
            public static IParsedResult<NullArguments> Parse(Action<IArgumentParserBuilder<NullArguments>> builder, string[] args, Action<IArgumentParserConfigurator>? configurator = null, ArgumentParserSettings settings = ArgumentParserSettings.CreateErrorsForUnparsed)
            {
                args.ValidateArgument(nameof(args));
                builder.ValidateArgument(nameof(builder));

                return Parse(builder, NullArguments.Instance, args, configurator, settings);
            }
        }
    }
}
