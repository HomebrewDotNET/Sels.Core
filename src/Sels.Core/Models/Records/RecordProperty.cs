using Sels.Core.Extensions.Conversion;
using static Sels.Core.Delegates;

namespace Sels.Core.Records
{
    /// <summary>
    /// A struct who's equality check is delegated to a delegate or always returns true when no delegate is provided. Useful in records for defining extra properties not to include in the equality check of the record or when custom equality checks are needed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct RecordProperty<T>
    {
        // Fields
        private readonly Comparator<T>? _comparator;

        /// <summary>
        /// The value of the property.
        /// </summary>
        public T? Value { get; }

        /// <inheritdoc cref="RecordProperty{T}"/>
        /// <param name="comparator">Optional delegate used during the equality check of the current instance</param>
        public RecordProperty(Comparator<T>? comparator = null) : this(default, comparator)
        {
            Value = default;
        }
        /// <inheritdoc cref="RecordProperty{T}"/>
        /// <param name="value"><inheritdoc cref="Value"/></param>
        /// <param name="comparator">Optional delegate used during the equality check of the current instance</param>
        public RecordProperty(T value, Comparator<T>? comparator = null)
        {
            _comparator = comparator;
            Value = value;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            T objToCompare = obj is RecordProperty<T> recordProperty ? recordProperty.Value : obj is T ? obj.CastTo<T>() : default;

            return _comparator != null ? _comparator.Invoke(Value, objToCompare) : true;
        }

        /// <summary>
        /// Checks if the current instance is actually equal to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">The object to compare to the current instance</param>
        /// <returns>True if the current instance is equal to <paramref name="obj"/>, otherwise false</returns>
        public bool ActualEquals(object? obj)
        {
            if (Value == null) return false;
            if (obj == null) return false;

            return base.Equals(obj);
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return Value?.ToString();
        }
    }
}
