using Sels.Core.Display;
using Sels.Core.Equality;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Collections;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;

namespace Sels.Core.Collections
{
    /// <summary>
    /// Wrapper of a <see cref="HashSet{T}"/> that offers different methods of equality.
    /// </summary>
    /// <typeparam name="T">Type of the objects in the hash set</typeparam>
    public class EqualityHashSet<T> : HashSet<T>
    {
        // Fields
        private readonly EqualityType _type;
        private readonly StringType _stringType;

        /// <inheritdoc cref="EqualityHashSet{T}"/>
        /// <param name="type"><inheritdoc cref="EqualityType"/></param>
        /// <param name="stringType"><inheritdoc cref="StringType"/></param>
        public EqualityHashSet(EqualityType type = EqualityType.Default, StringType stringType = StringType.Default)
        {
            _type = type;
            _stringType = stringType;
        }

        /// <inheritdoc cref="EqualityHashSet{T}"/>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        /// <param name="type"><inheritdoc cref="EqualityType"/></param>
        /// <param name="stringType"><inheritdoc cref="StringType"/></param>
        public EqualityHashSet(int capacity, EqualityType type = EqualityType.Default, StringType stringType = StringType.Default) : base(capacity)
        {
            _type = type;
            _stringType = stringType;
        }

        /// <inheritdoc cref="EqualityHashSet{T}"/>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        /// <param name="type"><inheritdoc cref="EqualityType"/></param>
        /// <param name="stringType"><inheritdoc cref="StringType"/></param>
        public EqualityHashSet(IEnumerable<T> collection, EqualityType type = EqualityType.Default, StringType stringType = StringType.Default) : base(collection)
        {
            _type = type;
            _stringType = stringType;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            switch (_type)
            {
                case EqualityType.None:
                case EqualityType.Default:
                    return base.GetHashCode();
                case EqualityType.Sequence:
                    int? currentNumber = null;
                    foreach (var number in this.Select((x, i) => i + x.GetHashCode()))
                    {
                        if (currentNumber == null)
                        {
                            currentNumber = number;
                            continue;
                        }
                        currentNumber = currentNumber ^ number;
                    }

                    return currentNumber ?? 0;
                case EqualityType.SequenceUnordered:
                    currentNumber = null;
                    foreach (var number in this.Select(x => x.GetHashCode()))
                    {
                        if (currentNumber == null)
                        {
                            currentNumber = number;
                            continue;
                        }
                        currentNumber = currentNumber ^ number;
                    }

                    return currentNumber ?? 0;
                default:
                    throw new NotSupportedException($"Type <{_type}> is currently not supported");
            }
        }
        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not IEnumerable<T>) return false;

            var collection = obj.CastTo<IEnumerable<T>>();

            switch (_type)
            {
                case EqualityType.Default:
                    return base.Equals(obj);
                case EqualityType.Sequence:
                    return collection.SequenceEqual(this);
                case EqualityType.SequenceUnordered:
                    return collection.UnorderedEquals(this);
                case EqualityType.None:
                    return true;
                default:
                    throw new NotSupportedException($"Type <{_type}> is currently not supported");
            }
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            switch (_stringType)
            {
                case StringType.Default:
                    return base.ToString();
                case StringType.Formatted:
                    return $"{typeof(T).GetDisplayName()}[{Count}]";
                case StringType.FormattedShort:
                    return $"{typeof(T).GetDisplayName(false)}[{Count}]";

                default:
                    throw new NotSupportedException($"Type <{_stringType}> is currently not supported");
            }
        }
    }
}
