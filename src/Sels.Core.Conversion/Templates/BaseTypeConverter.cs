using Microsoft.Extensions.Caching.Memory;
using Sels.Core.Conversion.Converters;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Collections;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;

namespace Sels.Core.Conversion.Templates
{
    /// <summary>
    /// Template for creating new type converters. Validates input so derived classes do not need to check for nulls and provides some helper methods.
    /// </summary>
    public abstract class BaseTypeConverter : ITypeConverter
    {
        #region Conversion
        /// <inheritdoc/>
        public virtual bool CanConvert(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            convertType.ValidateArgument(nameof(convertType));
            if (value == null) return false;

            return CanConvertObject(value, convertType, arguments);
        }
        /// <inheritdoc/>
        public virtual object ConvertTo(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null)
        {
            value.ValidateArgument(x => CanConvert(x, convertType, arguments), $"Converter <{this}> cannot convert using the provided value. Call <{nameof(CanConvert)}> first");

            return ConvertObjectTo(value, convertType, arguments);
        }

        /// <summary>
        /// Tries to get cache settings from <paramref name="arguments"/> if they were passed down by the caller.
        /// </summary>
        /// <param name="arguments">The arguments passed down by the caller</param>
        /// <param name="cacheSettings">The configured cache settings in <paramref name="arguments"/> if they were provided</param>
        /// <returns>True if cache setting were included in <paramref name="arguments"/>, otherwise false</returns>
        protected bool TryGetCache(IReadOnlyDictionary<string, object> arguments, out (IMemoryCache Cache, string CacheKeyPrefix, TimeSpan? Retention) cacheSettings)
        {
            cacheSettings = default;
            if(arguments == null) return false;

            var cache = arguments.ContainsKey(ConversionConstants.Converters.CacheArgument) ? arguments[ConversionConstants.Converters.CacheArgument].CastTo<IMemoryCache>() : null;
            if (cache == null) return false;

            var cachePrefix = arguments.ContainsKey(ConversionConstants.Converters.CacheKeyPrefixArgument) ? arguments[ConversionConstants.Converters.CacheKeyPrefixArgument].CastTo<string>() : null;
            var cacheRetention = arguments.ContainsKey(ConversionConstants.Converters.CacheRetentionArgument) ? arguments[ConversionConstants.Converters.CacheRetentionArgument].CastTo<TimeSpan>() : (TimeSpan?)null;

            cacheSettings = (cache, cachePrefix, cacheRetention);
            return true;
        }

        /// <inheritdoc cref="ITypeConverter.CanConvert(object, Type, IReadOnlyDictionary{string, object})"/>
        protected abstract bool CanConvertObject(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null);
        /// <inheritdoc cref="ITypeConverter.ConvertTo(object, Type, IReadOnlyDictionary{string, object})"/>
        protected abstract object ConvertObjectTo(object value, Type convertType, IReadOnlyDictionary<string, object> arguments = null);
        #endregion

        #region Helper
        /// <summary>
        /// Checks if <paramref name="left"/> and <paramref name="right"/> are a pair of <typeparamref name="TLeft"/> and <typeparamref name="TRight"/>. Useful when creating a converter that can convert between 2 types.
        /// </summary>
        /// <typeparam name="TLeft">First type that can be converted from/to</typeparam>
        /// <typeparam name="TRight">Second type that can be converted from/to</typeparam>
        /// <param name="left">First type to check</param>
        /// <param name="right">Second type to check</param>
        /// <param name="getNullableType">Set to true to get the underlying type if any of the types is nullable</param>
        /// <returns>Whether or not <paramref name="left"/> and <paramref name="right"/> are a pair of <typeparamref name="TLeft"/> and <typeparamref name="TRight"/></returns>
        protected bool AreTypePair<TLeft, TRight>(Type left, Type right, bool getNullableType = true)
        {
            left.ValidateArgument(nameof(left));
            right.ValidateArgument(nameof(right));

            left = getNullableType ? Nullable.GetUnderlyingType(left) ?? left : left;
            right = getNullableType ? Nullable.GetUnderlyingType(right) ?? right : right;

            return (left.IsAssignableTo<TLeft>() && right.IsAssignableTo<TRight>()) || (left.IsAssignableTo<TRight>() && right.IsAssignableTo<TLeft>());
        }
        /// <summary>
        /// Checks if <paramref name="left"/> and <paramref name="right"/> are a pair of based on conditions <paramref name="leftCondition"/> and <paramref name="rightCondition"/>. Useful when creating a converter that can convert between 2 types.
        /// </summary>
        /// <param name="left">First type to check</param>
        /// <param name="right">Second type to check</param>
        /// <param name="leftCondition">First condition for the type that can be converted from/to</param>
        /// <param name="rightCondition">Second condition for the type that can be converted from/to</param>
        /// <param name="getNullableType">Set to true to get the underlying type if any of the types is nullable</param>
        /// <returns>Whether or not <paramref name="left"/> and <paramref name="right"/> are a pair based on <paramref name="leftCondition"/> and <paramref name="rightCondition"/></returns>
        protected bool AreTypePair(Type left, Type right, Predicate<Type> leftCondition, Predicate<Type> rightCondition, bool getNullableType = true)
        {
            left.ValidateArgument(nameof(left));
            right.ValidateArgument(nameof(right));
            leftCondition.ValidateArgument(nameof(leftCondition));
            rightCondition.ValidateArgument(nameof(rightCondition));

            left = getNullableType ? Nullable.GetUnderlyingType(left) ?? left : left;
            right = getNullableType ? Nullable.GetUnderlyingType(right) ?? right : right;

            return (leftCondition(left) && rightCondition(right)) || (leftCondition(right) && rightCondition(left));
        }
        #endregion

    }
}
