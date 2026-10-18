using Sels.Core.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Entity
{
    /// <summary>
    /// Collection wrapper that can be used to compare collections for equivalence. The collection can be compared with other collections of the same type and item type.
    /// </summary>
    /// <typeparam name="TCollection">The list type wrapped by the collection.</typeparam>
    /// <typeparam name="TItem">The item type stored in the collection.</typeparam>
    public class EquivalentCollection<TCollection, TItem> : IDeduplicatable, IReadOnlyList<TItem>
        where TCollection : IReadOnlyList<TItem>
    {
        // Fields
        private readonly bool _itemIsDeduplicatable = typeof(IDeduplicatable).IsAssignableFrom(typeof(TItem));
        private readonly bool _strictOrder;
        private readonly TCollection _collection;

        /// <summary>
        /// The collection wrapped by this <see cref="EquivalentCollection{TCollection, TItem}"/>.
        /// </summary>
        public TCollection Collection => _collection;
        /// <inheritdoc/>
        public int Count => _collection.Count;
        /// <inheritdoc/>
        public TItem this[int index] => _collection[index];

        /// <inheritdoc cref="EquivalentCollection{TCollection, TItem}"/>
        /// <param name="collection">The collection to wrap</param>
        /// <param name="strictOrder">Whether the order of items should be considered when comparing for equivalence</param>
        public EquivalentCollection(TCollection collection, bool strictOrder = false)
        {
            _collection = collection.ValidateArgument(nameof(collection));
            _strictOrder = strictOrder;
        }

        /// <inheritdoc/>
        public int GetBucket()
        {
            return Collection.Count;
        }
        /// <inheritdoc/>
        public bool IsEquivalentTo(IDeduplicatable other)
        {
            if (!(other is EquivalentCollection<TCollection, TItem> otherCollection))
            {
                return false;
            }

            if (otherCollection.Count != Count)
            {
                return false;
            }

            if (_strictOrder)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (_itemIsDeduplicatable)
                    {
                        if (!((IDeduplicatable)this[i]).IsEquivalentTo((IDeduplicatable)otherCollection[i]))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!EqualityComparer<TItem>.Default.Equals(this[i], otherCollection[i]))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            else
            {
                if(_itemIsDeduplicatable)
                {
                    var otherDuplicates = new Deduplicator<TItem>(otherCollection._collection);
                    
                    for(int i = 0; i < Count; i++)
                    {
                        if(!otherDuplicates.HasDuplicate(this[i]))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    var sourceHashSet = new HashSet<TItem>(_collection);
                    var otherHashSet = new HashSet<TItem>(otherCollection._collection);
                    return sourceHashSet.SetEquals(otherHashSet);
                }
            }
            return true;
        }
        /// <inheritdoc/>
        public IEnumerator<TItem> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }
        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }
    }
}
