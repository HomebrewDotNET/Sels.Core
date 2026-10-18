using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Entity
{
    /// <summary>
    /// The keys used for lookup in <see cref="Metadata"/>. Provides better performance over <see cref="string"/>.
    /// Should be stored by ref for faster lookups.
    /// </summary>
    public class MetadataKey
    {
        // Statics
        /// <summary>
        /// The global cache for the metadata keys.
        /// </summary>
        protected readonly static Dictionary<string, MetadataKey> _cache = new Dictionary<string, MetadataKey>(StringComparer.OrdinalIgnoreCase);

        // Properties
        /// <summary>
        /// The name of the key.
        /// </summary>
        public string Name { get; }

        /// <inheritdoc cref="MetadataKey"/>
        /// <param name="name"><inheritdoc cref="Name"/></param>
        internal MetadataKey(string name)
        {
            Name = name.ValidateArgumentNotNullOrWhitespace(nameof(name));
        }

        /// <summary>
        /// Creates/returns the metadata key for <paramref name="name"/>.
        /// </summary>
        /// <param name="name"><inheritdoc cref="Name"/></param>
        /// <returns>The metadata key for <paramref name="name"/></returns>
        public static MetadataKey Get(string name)
        {
            name = name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            if (!_cache.TryGetValue(name, out MetadataKey key))
            {
                lock (_cache)
                {
                    if (!_cache.TryGetValue(name, out key))
                    {
                        key = new MetadataKey(name);
                        _cache.Add(name, key);
                    }
                }
            }
            return key;
        }
    }

    /// <summary>
    /// The keys used for lookup in <see cref="Metadata"/>. Provides better performance over <see cref="string"/>.
    /// Should be stored by ref for faster lookups.
    /// Typed version for selecting overloads in <see cref="Metadata"/>.
    /// </summary>
    public class MetadataKey<T> : MetadataKey
    {
        private MetadataKey(string name) : base(name)
        {
        }

        /// <summary>
        /// Creates/returns the metadata key for <paramref name="name"/>.
        /// </summary>
        /// <param name="name"><inheritdoc cref="MetadataKey.Name"/></param>
        /// <returns>The metadata key for <paramref name="name"/></returns>
        public static new MetadataKey<T> Get(string name)
        {
            name = name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            if (!_cache.TryGetValue(name, out MetadataKey key))
            {
                lock (_cache)
                {
                    if (!_cache.TryGetValue(name, out key))
                    {
                        key = new MetadataKey<T>(name);
                        _cache.Add(name, key);
                    }
                }
            }
            if (!(key is MetadataKey<T> typedKey)) throw new InvalidOperationException($"Metadata key {name} is defined but not of expected type {typeof(T)}. The same key might be used accross multiple types");
            return typedKey;
        }
    }
}
