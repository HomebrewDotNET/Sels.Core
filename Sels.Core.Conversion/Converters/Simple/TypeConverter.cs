using Microsoft.Extensions.Caching.Memory;
using Sels.Core.Conversion.Templates;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Collections;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sels.Core.Conversion.Converters.Simple
{
    /// <summary>
    /// Converts betweeen <see cref="string"/> and <see cref="Type"/>.
    /// </summary>
    public class TypeConverter : BaseTypeConverter
    {
        // Constants
        /// <summary>
        /// Argument used to ignore the assembly version when converting from a string. Default is true.
        /// </summary>
        public const string IgnoreVersionArgument = "Type.IgnoreAssemblyVersion";


        /// <inheritdoc/>
        protected override bool CanConvertObject(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            return AreTypePair<string, Type>(value.GetType(), convertType);
        }
        /// <inheritdoc/>
        protected override object ConvertObjectTo(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            if(value is Type type && convertType.IsString())
            {
                return type.AssemblyQualifiedName;
            }
            else if (value is string typeName && convertType.IsAssignableTo<Type>())
            {
                bool ignoreVersion = arguments.HasValue() && arguments.TryGetValue<bool>(IgnoreVersionArgument, out var ignore) ? ignore : true;
                
                if(TryGetCache(arguments, out var cacheSettings))
                {
                    var cache = cacheSettings.Cache;
                    var cacheKey = cacheSettings.CacheKeyPrefix.HasValue() ? $"{cacheSettings.CacheKeyPrefix}.TypeConversion.{typeName}" : $"{GetType().FullName}.TypeConversion.{typeName}";
                    var cacheRetention = cacheSettings.Retention.HasValue ? cacheSettings.Retention.Value : TimeSpan.FromMinutes(1);

                    return cache.GetOrCreate(cacheKey, x =>
                    {
                        x.SlidingExpiration = cacheRetention;

                        return GetType(typeName, ignoreVersion);
                    });
                }

                return GetType(typeName, ignoreVersion);
            }

            throw new NotSupportedException($"Can not convert <{value}> to type <{convertType}>");
        }

        private Type GetType(string typeName, bool ignoreVersion)
        {
            return Type.GetType(typeName, ignoreVersion ? x =>
            {
                x.Version = null;
                return Assembly.Load(x);
            }
            : (Func<AssemblyName, Assembly>)null, null, true);
        }
    }
}
