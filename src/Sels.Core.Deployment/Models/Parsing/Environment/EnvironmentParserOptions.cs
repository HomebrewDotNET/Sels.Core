using System.Collections;

namespace Sels.Core.Deployment.Parsing.Environment
{
    /// <summary>
    /// Exposes options that modifies the behaviour of an environment parser.
    /// </summary>
    [Flags]
    public enum EnvironmentParserOptions
    {
        /// <summary>
        /// Use the default behaviours.
        /// </summary>
        None = 0,
        /// <summary>
        /// Instead of throwing an exception if converting from the environment variable value to the property type fails, the parser will ignore the property instead.
        /// </summary>
        IgnoreConversionErrors = 1,
        /// <summary>
        /// Uppercase the expected environment variables names when searching for them.
        /// </summary>
        UppercaseNames = 2,
        /// <summary>
        /// By default properties that are assignable to <see cref="IEnumerable"/> will handled differently. Elements for the collection will be searched for by appending a counter starting at 1 behind the environment variable name.
        /// (e.g. With an environment variable name 'MyApp.Sources' the elements can be defined as: MyApp.Sources.1: Source 1, MyApp.Sources.2: Source 2, MyApp.Sources.3: Source 3)
        /// This behaviour will be disabled. The fallback behaviour of splitting the env variable will be used instead.
        /// (e.g. With an environment variable name 'MyApp.Sources' the elements can be defined as: MyApp.Source: Source 1; Source 2; Source 3)
        /// </summary>
        IgnoreMultiVariableArrays = 4
    }
}
