using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Equality;

namespace Sels.Core.Entity
{
    /// <summary>
    /// Deduplicator that can be used to store items and check for duplicates. Items are stored in buckets based on their hash code and can be compared for equivalence using the <see cref="IDeduplicatable"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of items to store in the deduplicator</typeparam>
    public class Deduplicator<T>
    {
        // Fields
        private readonly Dictionary<int, List<T>> _buckets;

        // Properties
        /// <summary>
        /// Deduplicate an object by a specific property. If null, the object will be deduplicated by its hash code.
        /// </summary>
        public Func<T, object> By { get; private set; }

        /// <inheritdoc cref="Deduplicator{T}"/>
        /// <param name="initial">Optional initial items to seed</param>
        /// <param name="by"><inheritdoc cref="By"/></param>
        public Deduplicator(IEnumerable<T> initial = null, Func<T, object> by = null)
        {
            By = by;
            if (initial != null)
            {
                var count = initial.TryGetCount();
                _buckets = count.HasValue ? new Dictionary<int, List<T>>(count.Value) : new Dictionary<int, List<T>>();
                foreach (var item in initial)
                {
                    Add(item);
                }
            }
            else
            {
                _buckets = new Dictionary<int, List<T>>();
            }
        }

        /// <summary>
        /// Adds an item to the deduplicator.
        /// </summary>
        /// <param name="item">The item to add</param>
        public virtual int Add(T item)
        {
            return AddItem(item);
        }

        /// <summary>
        /// Updates an item in the deduplicator by removing it from its old bucket and adding it to its new bucket.
        /// </summary>
        /// <param name="oldBucket">The bucket the item was previously in</param>
        /// <param name="item">The item to update</param>
        public virtual int Update(int oldBucket, T item)
        {
            item = item.ValidateArgument(nameof(item));

            if (_buckets.TryGetValue(oldBucket, out var list))
            {
                list.Remove(item);
                if (list.Count == 0)
                {
                    _buckets.Remove(oldBucket);
                }
            }
            return AddItem(item);
        }

        private int AddItem(T item)
        {
            item = item.ValidateArgument(nameof(item));

            var bucket = GetItemBucket(item);
            if (!_buckets.TryGetValue(bucket, out var list))
            {
                list = new List<T>();
                _buckets[bucket] = list;
            }
            list.Add(item);
            return bucket;
        }

        /// <summary>
        /// Removes an item from the deduplicator.
        /// </summary>
        /// <param name="item">The item to remove</param>
        public virtual void Remove(T item)
        {
            item = item.ValidateArgument(nameof(item));

            var bucket = GetItemBucket(item);
            if (_buckets.TryGetValue(bucket, out var list))
            {
                list.Remove(item);
                if (list.Count == 0)
                {
                    _buckets.Remove(bucket);
                }
            }
        }

        /// <summary>
        /// Checks if the deduplicator contains a duplicate of the given item.
        /// </summary>
        /// <param name="item">The item to check for duplicates</param>
        /// <returns>True if a duplicate exists, false otherwise</returns>
        public bool HasDuplicate(T item)
        {
            item = item.ValidateArgument(nameof(item));

            var bucket = GetItemBucket(item);
            if (_buckets.TryGetValue(bucket, out var list))
            {
                for(int i = 0; i < list.Count; i++)
                {
                    if (IsItemEquivalent(list[i], item))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets all duplicates of the given item in the deduplicator.
        /// </summary>
        /// <param name="item">The item to get duplicates for</param>
        /// <returns>An enumerable of all duplicates of the given item</returns>
        public IEnumerable<T> GetDuplicates(T item)
        {
            item = item.ValidateArgument(nameof(item));

            var bucket = GetItemBucket(item);
            if (_buckets.TryGetValue(bucket, out var list))
            {
                foreach (var existingItem in list)
                {
                    if (IsItemEquivalent(existingItem, item))
                    {
                        yield return existingItem;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the bucket for the given item. If the item implements <see cref="IDeduplicatable"/>, it will use the <see cref="IDeduplicatable.GetBucket"/> method to get the bucket. Otherwise, it will use the item's hash code.
        /// </summary>
        /// <param name="item">The item to get the bucket for</param>
        /// <returns>The bucket for the given item</returns>
        protected int GetItemBucket(T item)
        {
            item = item.ValidateArgument(nameof(item));

            var value = By?.Invoke(item) ?? item;
            if (value is IDeduplicatable deduplicatable)
            {
                return deduplicatable.GetBucket();
            }
            else
            {
                return value.GetHashCode();
            }
        }

        private bool IsItemEquivalent(T item1, T item2)
        {
            var value1 = By?.Invoke(item1) ?? item1;
            var value2 = By?.Invoke(item2) ?? item2;
            if (value1 is IDeduplicatable deduplicatable1 && value2 is IDeduplicatable deduplicatable2)
            {
                bool areSameReference = ReferenceEquals(deduplicatable1, deduplicatable2);
                return !areSameReference && deduplicatable1.IsEquivalentTo(deduplicatable2);
            }
            else
            {
                if(By != null)
                {
                    bool areSameReference = ReferenceEquals(item1, item2);
                    return !areSameReference && value1?.Equals(value2) == true;
                }
                return EqualityComparer<T>.Default.Equals(item1, item2);
            }
        }
    }
}
