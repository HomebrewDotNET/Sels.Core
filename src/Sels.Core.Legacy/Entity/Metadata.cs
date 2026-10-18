using Sels.Core.Extensions;
using Sels.Core.Extensions.Collections;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Entity
{
    /// <summary>
    /// A bag of metadata that can be used to store extra information without boxing (common) primitives.
    /// </summary>
    public struct Metadata
    {
        // State
        private Dictionary<MetadataKey, object> _objMetadata;
        private Dictionary<Type, object> _valueMetadata;
        private Dictionary<MetadataKey, int> _intMetadata;
        private Dictionary<MetadataKey, float> _floatMetadata;
        private Dictionary<MetadataKey, bool> _boolMetadata;
        private Dictionary<MetadataKey, long> _longMetadata;
        private Dictionary<MetadataKey, double> _doubleMetadata;
        private Dictionary<MetadataKey, decimal> _decimalMetadata;
        private Dictionary<MetadataKey, char> _charMetadata;
        private Dictionary<MetadataKey, DateTime> _dateTimeMetadata;
        private Dictionary<MetadataKey, TimeSpan> _timeSpanMetadata;
        private Dictionary<MetadataKey, Guid> _guidMetadata;
        private Dictionary<MetadataKey, byte> _byteMetadata;
        private Dictionary<MetadataKey, short> _shortMetadata;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Dictionary<MetadataKey, T> CreateMetadata<T>()
        {
            return new Dictionary<MetadataKey, T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object GetMetadata<T>(bool create)
        {
            switch (typeof(T))
            {
                case Type t when t == typeof(int):
                    if (_intMetadata is null && create) _intMetadata = CreateMetadata<int>();
                    return _intMetadata;
                case Type t when t == typeof(bool):
                    if (_boolMetadata is null && create) _boolMetadata = CreateMetadata<bool>();
                    return _boolMetadata;
                case Type t when t == typeof(float):
                    if (_floatMetadata is null && create) _floatMetadata = CreateMetadata<float>();
                    return _floatMetadata;
                case Type t when t == typeof(long):
                    if (_longMetadata is null && create) _longMetadata = CreateMetadata<long>();
                    return _longMetadata;
                case Type t when t == typeof(double):
                    if (_doubleMetadata is null && create) _doubleMetadata = CreateMetadata<double>();
                    return _doubleMetadata;
                case Type t when t == typeof(decimal):
                    if (_decimalMetadata is null && create) _decimalMetadata = CreateMetadata<decimal>();
                    return _decimalMetadata;
                case Type t when t == typeof(char):
                    if (_charMetadata is null && create) _charMetadata = CreateMetadata<char>();
                    return _charMetadata;
                case Type t when t == typeof(DateTime):
                    if (_dateTimeMetadata is null && create) _dateTimeMetadata = CreateMetadata<DateTime>();
                    return _dateTimeMetadata;
                case Type t when t == typeof(TimeSpan):
                    if (_timeSpanMetadata is null && create) _timeSpanMetadata = CreateMetadata<TimeSpan>();
                    return _timeSpanMetadata;
                case Type t when t == typeof(Guid):
                    if (_guidMetadata is null && create) _guidMetadata = CreateMetadata<Guid>();
                    return _guidMetadata;
                case Type t when t == typeof(byte):
                    if (_byteMetadata is null && create) _byteMetadata = CreateMetadata<byte>();
                    return _byteMetadata;
                case Type t when t == typeof(short):
                    if (_shortMetadata is null && create) _shortMetadata = CreateMetadata<short>();
                    return _shortMetadata;
                case Type t when t.IsValueType:
                    object valueDict = null;
                    if (!(_valueMetadata?.TryGetValue(t, out valueDict) ?? false))
                    {
                        if (create)
                        {
                            valueDict = CreateMetadata<T>();
                            if(_valueMetadata is null) _valueMetadata = new Dictionary<Type, object>();
                            _valueMetadata.Add(t, valueDict);
                        }
                    }
                    return valueDict;
                default:
                    if (_objMetadata is null && create) _objMetadata = CreateMetadata<object>();
                    return _objMetadata;
            }
        }

        /// <summary>
        /// Checks if the current metadata contains <paramref name="key"/> of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the metadata</typeparam>
        /// <param name="key">The key of the metadata</param>
        /// <returns>True when present, otherwise false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey<T>(MetadataKey key)
        {
            key = key.ValidateArgument(nameof(key));
            var dict = GetMetadata<T>(false);
            if (dict is Dictionary<MetadataKey, T> typedDict)
            {
                return typedDict.ContainsKey(key);
            }

            if (dict is Dictionary<MetadataKey, object> genericDict)
            {
                return genericDict.ContainsKey(key);
            }
            return false;
        }
        /// <inheritdoc cref="ContainsKey{T}(MetadataKey)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey<T>(MetadataKey<T> key)
        {
            return ContainsKey<T>((MetadataKey)key);
        }

        /// <summary>
        /// Tries to get value <paramref name="key"/> of type <typeparamref name="T"/> if it exists.
        /// </summary>
        /// <typeparam name="T">The type of the metadata</typeparam>
        /// <param name="key">The key of the metadata</param>
        /// <param name="value">The metadata value if found</param>
        /// <returns>True if the value was found, otherwise false</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue<T>(MetadataKey key, out T value)
        {
            key = key.ValidateArgument(nameof(key));
            var dict = GetMetadata<T>(false);
            value = default;
            if (dict is Dictionary<MetadataKey, T> typedDict)
            {
                return typedDict.TryGetValue(key, out value) ? true : false;
            }

            if (dict is Dictionary<MetadataKey, object> genericDict)
            {
                if (genericDict.TryGetValue(key, out var genericValue))
                {
                    if (genericValue is T typedValue)
                    {
                        value = typedValue;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <inheritdoc cref="TryGetValue{T}(MetadataKey, out T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue<T>(MetadataKey<T> key, out T value)
        {
            return TryGetValue<T>((MetadataKey)key, out value);
        }

        /// <summary>
        /// Sets metadata <paramref name="key"/> if type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the metadata</typeparam>
        /// <param name="key">The key of the metadata</param>
        /// <param name="value">The metadata value to set</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(MetadataKey key, T value)
        {
            key = key.ValidateArgument(nameof(key));
            var dict = GetMetadata<T>(true);

            if (dict is Dictionary<MetadataKey, T> typedDict)
            {
                typedDict.AddOrUpdate(key, value);
                return;
            }

            if (dict is Dictionary<MetadataKey, object> genericDict)
            {
                genericDict.AddOrUpdate(key, value);
                return;
            }
        }
        /// <inheritdoc cref="Set{T}(MetadataKey, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(MetadataKey<T> key, T value)
        {
            Set<T>((MetadataKey)key, value);
        }

        /// <summary>
        /// Removes metadata <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">The type of the metadata</typeparam>
        /// <param name="key">The key of the metadata</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unset<T>(MetadataKey key)
        {
            key = key.ValidateArgument(nameof(key));
            var dict = GetMetadata<T>(false);

            if (dict is Dictionary<MetadataKey, T> typedDict)
            {
                _ = typedDict.Remove(key);
                return;
            }

            if (dict is Dictionary<MetadataKey, object> genericDict)
            {
                _ = genericDict.Remove(key);
                return;
            }
        }
        /// <inheritdoc cref="Unset{T}(MetadataKey)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unset<T>(MetadataKey<T> key)
        {
            Unset<T>((MetadataKey)key);
        }
    }
}
