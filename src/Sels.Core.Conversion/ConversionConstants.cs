using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Conversion
{
    /// <summary>
    /// Contains static read-only/constant values related to conversion.
    /// </summary>
    public static class ConversionConstants
    {
        /// <summary>
        /// Contains static read-only/constant values related to type converters.
        /// </summary>
        public static class Converters
        {
            /// <summary>
            /// Argument that can be used to pass down a <see cref="IMemoryCache"/> to converters so they can cache converted values to improve performance.
            /// Argument value is <see cref="IMemoryCache"/>.
            /// </summary>
            public const string CacheArgument = "TypeConverters.Cache";
            /// <summary>
            /// Argument that can be used to define a prefix that will be used for all cache keys created by converters.
            /// Argument value is <see cref="string"/>.
            /// </summary>
            public const string CacheKeyPrefixArgument = "TypeConverters.CachePrefix";
            /// <summary>
            /// Argument that can be used to overwrite the default retention of cached values by type converters.
            /// Argument value is <see cref="TimeSpan"/>.
            /// </summary>
            public const string CacheRetentionArgument = "TypeConverters.CacheRetention";
        }
    }
}
