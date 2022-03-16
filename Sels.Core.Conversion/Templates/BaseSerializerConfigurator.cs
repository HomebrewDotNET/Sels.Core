using Microsoft.Extensions.Logging;
using Sels.Core.Conversion;
using Sels.Core.Conversion.Converters;
using Sels.Core.Conversion.Serialization.Filters;
using Sels.Core.Conversion.Serializers;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging.Advanced;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sels.Core.Conversion.Templates
{
    /// <summary>
    /// Template for creating new configurators.
    /// </summary>
    /// <typeparam name="TConfigurator">The type of the configurator inheriting from this class</typeparam>
    public abstract class BaseSerializerConfigurator<TConfigurator> : ISerializerConfigurator<TConfigurator>
    {
        // Fields
        /// <summary>
        /// The registered loggers.
        /// </summary>
        protected readonly List<ILogger> _loggers = new List<ILogger>();
        /// <summary>
        /// The registered converters.
        /// </summary>
        protected readonly List<ITypeConverter> _converters = new List<ITypeConverter>();
        /// <summary>
        /// The registered filters.
        /// </summary>
        protected readonly List<ISerializationFilter> _filters = new List<ISerializationFilter>();
        /// <summary>
        /// The registered element filters.
        /// </summary>
        protected readonly List<ISerializationFilter> _elementFilters = new List<ISerializationFilter>();

        // Properties
        /// <summary>
        /// The configured loggers.
        /// </summary>
        public ILogger[] Loggers => _loggers.ToArray();
        /// <summary>
        /// The configured converters.
        /// </summary>
        public ITypeConverter[] Converters => _converters.ToArray();
        /// <summary>
        /// The configured filters.
        /// </summary>
        public ISerializationFilter[] Filters => _filters.ToArray();
        /// <summary>
        /// The configured element filters.
        /// </summary>
        public ISerializationFilter[] ElementFilters => _elementFilters.ToArray();
        /// <summary>
        /// The configured binding flags.
        /// </summary>
        public BindingFlags PropertyFlags { get; private set; } = BindingFlags.Instance | BindingFlags.Public;

        /// <inheritdoc/>
        public TConfigurator UseConverters(params ITypeConverter[] converters)
        {
            using (_loggers.TraceMethod(this))
            {
                converters.ValidateArgument(nameof(converters));
                converters.Execute((i, x) => x.ValidateArgument(a => a != null, $"Converter <{i}> was null"));

                _converters.AddRange(converters);

                return Instance;
            }
        }
        /// <inheritdoc/>
        public TConfigurator UseFilters(params ISerializationFilter[] filters)
        {
            using (_loggers.TraceMethod(this))
            {
                filters.ValidateArgument(nameof(filters));
                filters.Execute((i, x) => x.ValidateArgument(a => a != null, $"Filter <{i}> was null"));

                _filters.AddRange(filters);

                return Instance;
            }
        }
        /// <inheritdoc/>
        public TConfigurator UseElementFilters(params ISerializationFilter[] filters)
        {
            using (_loggers.TraceMethod(this))
            {
                filters.ValidateArgument(nameof(filters));
                filters.Execute((i, x) => x.ValidateArgument(a => a != null, $"Filter <{i}> was null"));

                _elementFilters.AddRange(filters);

                return Instance;
            }
        }
        /// <inheritdoc/>
        public TConfigurator UseLoggers(params ILogger[] loggers)
        {
            using (_loggers.TraceMethod(this))
            {
                loggers.ValidateArgument(nameof(loggers));
                loggers.Execute((i, x) => x.ValidateArgument(a => a != null, $"Logger <{i}> was null"));

                _loggers.AddRange(_loggers);

                return Instance;
            }
        }
        /// <inheritdoc/>
        public TConfigurator ForProperties(BindingFlags bindingFlags)
        {
            using (_loggers.TraceMethod(this))
            {
                PropertyFlags = bindingFlags;
                return Instance;
            }
        }

        // Abstractions
        /// <summary>
        /// The instance to return for the method chaining.
        /// </summary>
        public abstract TConfigurator Instance { get; }
    }
}
