using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Cli.ArgumentParsing
{
    /// <inheritdoc cref="IOptionHandlerBuilder"/>
    internal class OptionHandlerBuilder : IOptionHandlerBuilder
    {
        // Properties
        /// <summary>
        /// The configured short option prefix.
        /// </summary>
        public char? OptionPrefix { get; private set; }
        /// <summary>
        /// The configured long option prefix.
        /// </summary>
        public string? LongOptionPrefix { get; private set; }
        /// <summary>
        /// If the current option can be declared multiple times.
        /// </summary>
        public bool DuplicateAllowed { get; private set; }
        /// <summary>
        /// The custom pattern configured.
        /// </summary>
        public string? Format { get; private set; }
        /// <summary>
        /// The configured delegate that create the value to parse in case the option is a flag.
        /// </summary>
        public Func<object>? DefinedValueConstructor { get; private set; }

        /// <inheritdoc cref="OptionHandlerBuilder"/>
        /// <param name="configurator">The delegate that will configure the current instance</param>
        public OptionHandlerBuilder(Action<IOptionHandlerBuilder>? configurator)
        {
            if(configurator != null) configurator(this);
        }

        /// <inheritdoc/>
        public IOptionHandlerBuilder AllowDuplicate()
        {
            DuplicateAllowed = true;
            return this;
        }
        /// <inheritdoc/>
        public IOptionHandlerBuilder FromFormat(string pattern)
        {
            pattern.ValidateArgumentNotNullOrWhitespace(nameof(pattern));
            pattern.ValidateArgument(x => !x.ContainsWhitespace(), $"{nameof(pattern)} cannot contain any whitespace characters");
            pattern.ValidateArgument(x => !x.Contains(CliConstants.ArgumentParsing.OptionPatternOptionName), $"{nameof(pattern)} does not contain {CliConstants.ArgumentParsing.OptionPatternOptionName}");
            pattern.ValidateArgument(x => !x.Contains(CliConstants.ArgumentParsing.OptionPatternArgName), $"{nameof(pattern)} does not contain {CliConstants.ArgumentParsing.OptionPatternArgName}");

            Format = pattern;
            return this;
        }
        /// <inheritdoc/>
        public IOptionHandlerBuilder WhenDefined(Func<object> valueConstructor)
        {
            valueConstructor.ValidateArgument(nameof(valueConstructor));

            DefinedValueConstructor = valueConstructor;
            return this;
        }
        /// <inheritdoc/>
        public IOptionHandlerBuilder WhenDefined(object value)
        {
            DefinedValueConstructor = () => value;
            return this;
        }
        /// <inheritdoc/>
        public IOptionHandlerBuilder WithPrefix(char shortOptionPrefix)
        {
            OptionPrefix = shortOptionPrefix;
            return this;
        }
        /// <inheritdoc/>
        public IOptionHandlerBuilder WithPrefix(string longOptionPrefix)
        {
            longOptionPrefix.ValidateArgumentNotNullOrWhitespace(nameof(longOptionPrefix));
            longOptionPrefix.ValidateArgument(x => !x.ContainsWhitespace(), $"{nameof(longOptionPrefix)} cannot contain any whitespace characters");
           
            LongOptionPrefix = longOptionPrefix;
            return this;
        }
    }
}
