using Microsoft.Extensions.Logging;
using Sels.Core.Conversion.Converters;
using System.Reflection;

namespace Sels.Core.Deployment.Parsing.Environment
{
    /// <summary>
    /// Contains the config for an environment parser.
    /// </summary>
    internal class ParserConfig : IEnvironmentParserConfigurator
    {
        // Fields
        private EnvironmentParserOptions _options = EnvironmentParserOptions.None;
        private readonly List<Predicate<PropertyInfo>> _ignoredCollectionProperties = new List<Predicate<PropertyInfo>>();
        private readonly List<Predicate<PropertyInfo>> _ignoredFallthroughProperties = new List<Predicate<PropertyInfo>>();

        // Properties
        /// <summary>
        /// Optional logger for tracing.
        /// </summary>
        public ILogger? Logger { get; private set; }
        /// <summary>
        /// The converter to use.
        /// </summary>
        public ITypeConverter Converter { get; private set; } = GenericConverter.DefaultConverter;
        /// <summary>
        /// The prefix to use for environment variable names.
        /// </summary>
        public string? Prefix { get; private set; }
        /// <summary>
        /// The character to use to split a string into different key/value pairs used as properties for an object.
        /// </summary>
        public char Splitter { get; private set; } = ';';
        /// <summary>
        /// If conversion errors can be ignored.
        /// </summary>
        public bool IgnoreConversionErrors => _options.HasFlag(EnvironmentParserOptions.IgnoreConversionErrors);
        /// <summary>
        /// If environment variables names need to be uppercased when searching for them.
        /// </summary>
        public bool ToUppercaseNames => _options.HasFlag(EnvironmentParserOptions.UppercaseNames);
        /// <summary>
        /// Disables the default behaviour of handling elements using multiple env variables.
        /// </summary>
        public bool IgnoreMultiVariableArrays => _options.HasFlag(EnvironmentParserOptions.IgnoreMultiVariableArrays);

        /// <inheritdoc cref="ParserConfig"/>
        /// <param name="configurator">Option delegate to configure this instance</param>
        public ParserConfig(Action<IEnvironmentParserConfigurator>? configurator = null)
        {
            configurator?.Invoke(this);
        }

        /// <summary>
        /// Checks if <paramref name="property"/> is ignored as a collection.
        /// </summary>
        /// <param name="property">The property to check</param>
        /// <returns>True if <paramref name="property"/> is ignored, otherwise false</returns>
        public bool IsIgnoredAsCollection(PropertyInfo property)
        {
            property.ValidateArgument(nameof(property));

            return _ignoredCollectionProperties.HasValue() && _ignoredCollectionProperties.Any(x => x(property));
        }
        /// <summary>
        /// Checks if we are allowed to check the sub properties of <paramref name="property"/>.
        /// </summary>
        /// <param name="property">The property to check</param>
        /// <returns>True if we are allowed to check <paramref name="property"/>, otherwise false</returns>
        public bool CanFallthrough(PropertyInfo property)
        {
            property.ValidateArgument(nameof(property));

            return !_ignoredFallthroughProperties.HasValue() || !_ignoredFallthroughProperties.Any(x => x(property));
        }

        /// <inheritdoc/>
        IEnvironmentParserConfigurator IEnvironmentParserConfigurator.UseConverter(ITypeConverter converter)
        {
            Converter = converter.ValidateArgument(nameof(converter));
            return this;
        }
        /// <inheritdoc/>
        IEnvironmentParserConfigurator IEnvironmentParserConfigurator.UseLogger(ILogger? logger)
        {
            Logger = logger;
            return this;
        }
        /// <inheritdoc/>
        IEnvironmentParserConfigurator IEnvironmentParserConfigurator.UseOptions(EnvironmentParserOptions options)
        {
            _options = options;
            return this;
        }
        /// <inheritdoc/>
        IEnvironmentParserConfigurator IEnvironmentParserConfigurator.UsePrefix(string? prefix)
        {
            Prefix = prefix;
            return this;
        }
        /// <inheritdoc/>
        IEnvironmentParserConfigurator IEnvironmentParserConfigurator.UseSplitter(char splitter)
        {
            Splitter = splitter;
            return this;
        }
        /// <inheritdoc/>
        IEnvironmentParserConfigurator IEnvironmentParserConfigurator.IgnoreAsCollection(Predicate<PropertyInfo> predicate)
        {
            predicate.ValidateArgument(nameof(predicate));
            _ignoredCollectionProperties.Add(predicate);
            return this;
        }
        /// <inheritdoc/>
        IEnvironmentParserConfigurator IEnvironmentParserConfigurator.IgnoreSubProperties(Predicate<PropertyInfo> predicate)
        {
            predicate.ValidateArgument(nameof(predicate));
            _ignoredFallthroughProperties.Add(predicate);
            return this;
        }
    }
}
