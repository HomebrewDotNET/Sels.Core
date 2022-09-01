using Microsoft.Extensions.Logging;
using Sels.Core.Conversion.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Deployment.Parsing.Environment
{
    /// <summary>
    /// Exposes extra settings/options for a environment parser.
    /// </summary>
    public interface IEnvironmentParserConfigurator
    {
        /// <summary>
        /// Adds an optional logger that allows the parser to log.
        /// </summary>
        /// <param name="logger">The logger instance to use</param>
        /// <returns>Current builder for method chaining</returns>
        IEnvironmentParserConfigurator UseLogger(ILogger? logger);
        /// <summary>
        /// Defines a custom converter to used instead of the default one.
        /// The converter is used to convert the environment variable value to the property type.
        /// </summary>
        /// <param name="converter">The converter to use</param>
        /// <returns>Current builder for method chaining</returns>
        IEnvironmentParserConfigurator UseConverter(ITypeConverter converter);
        /// <summary>
        /// Defines a prefix that will be added in front of each expected environment variable name.
        /// (e.g. with a prefix of 'MyApp' and an expected name of 'AppSettings.DevMode' the parser will search for an environment variable with name 'MyApp.AppSettings.DevMode')
        /// </summary>
        /// <param name="prefix">The prefix to use</param>
        /// <returns>Current builder for method chaining</returns>
        IEnvironmentParserConfigurator UsePrefix(string? prefix);
        /// <summary>
        /// Define the options to use for this parser.
        /// </summary>
        /// <returns>Current builder for method chaining</returns>
        IEnvironmentParserConfigurator UseOptions(EnvironmentParserOptions options);
        /// <summary>
        /// Overwrites the default splitter used to split a string into multiple elements. Default is ;
        /// (e.g. With a splitter ; an object can be defined as: PropertyOneName:{Value};PropertyTwoName:{Value};PropertyTwoName:{Value})
        /// (e.g. With a splitter ; an array can be defined as: Element1; Element2; Element3)
        /// </summary>
        /// <param name="splitter">The character to use</param>
        /// <returns>Current builder for method chaining</returns>
        IEnvironmentParserConfigurator UseSplitter(char splitter);
        /// <summary>
        /// Adds a predicate that checks if a property is ignored as a collection.
        /// This causes the property value to be parsed as a regular value instead of a collection.
        /// (e.g. string is parsed as a string instead of as a char[])
        /// Can be called multiple times.
        /// </summary>
        /// <param name="predicate">Predicate that dictates if a property is ignored as a collection</param>
        /// <returns>Current builder for method chaining</returns>
        IEnvironmentParserConfigurator IgnoreAsCollection(Predicate<PropertyInfo> predicate);
        /// <summary>
        /// Adds that predicate that dictates if sub properties are allowed to be checked.
        /// This causes the parser to ignore sub properties if the value couldn't be parsed.
        /// (e.g. Don't check sub properties on List{T} if it couldn't be parsed)
        /// Can be called multiple times.
        /// </summary>
        /// <param name="predicate">Predicate that dictates if sub properties aren't checked</param>
        /// <returns>Current builder for method chaining</returns>
        IEnvironmentParserConfigurator IgnoreSubProperties(Predicate<PropertyInfo> predicate);
    }
}
