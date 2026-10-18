using Sels.Core.Equality;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace Sels.Core.Entity.Components
{
    /// <summary>
    /// A <see cref="Deduplicator{T}"/> that notifies when the collection changes or when any of the added items changes.
    /// Subscribes to added items that implement <see cref="INotifyPropertyChanged"/> or <see cref="INotifyCollectionChanged"/> and raises the appropriate events when they change.
    /// </summary>
    /// <typeparam name="T">The type of object to deduplicate</typeparam>
    public class ObservableDeduplicator<T> : Deduplicator<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        // Fields
        private readonly Dictionary<T, (int Bucket, PropertyChangedEventHandler Subscriber, NotifyCollectionChangedEventHandler CollectionSubscriber)> _items = new Dictionary<T, (int Bucket, PropertyChangedEventHandler Subscriber, NotifyCollectionChangedEventHandler CollectionSubscriber)>(ReferenceComparer<T>.Default);

        // State
        private List<NotifyCollectionChangedEventHandler> _watched;
        private List<PropertyChangedEventHandler> _propertyWatched;

        // Properties
        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc cref="ObservableDeduplicator{T}"/>
        /// <param name="initial">Optional initial items to seed</param>
        /// <param name="by"><inheritdoc cref="Deduplicator{T}.By"/></param>
        public ObservableDeduplicator(IEnumerable<T> initial = null, Func<T, object> by = null) : base(initial, by)
        {
        }

        /// <inheritdoc/>
        public override int Add(T item)
        {
            if (_items.ContainsKey(item))
            {
                throw new ArgumentException($"Item '{item}' is already in the deduplicator", nameof(item));
            }
            var bucket = base.Add(item);

            PropertyChangedEventHandler subscriber = null;
            if(item is INotifyPropertyChanged notifyPropertyChanged)
            {
                subscriber = new PropertyChangedEventHandler((sender, args) =>
                {
                    PropertyChanged?.Invoke(sender, args);
                });
                notifyPropertyChanged.PropertyChanged += (PropertyChangedEventHandler)subscriber;
            }
            NotifyCollectionChangedEventHandler collectionSubscriber = null;
            if(item is INotifyCollectionChanged notifyCollectionChanged)
            {
                collectionSubscriber = new NotifyCollectionChangedEventHandler((sender, args) =>
                {
                    CollectionChanged?.Invoke(sender, args);
                });
                notifyCollectionChanged.CollectionChanged += (NotifyCollectionChangedEventHandler)collectionSubscriber;
            }

            _items.Add(item, (bucket, subscriber, collectionSubscriber));

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            return bucket;
        }

        /// <inheritdoc/>
        public override int Update(int oldBucket, T item)
        {
            var bucket = base.Update(oldBucket, item);

            if (_items.TryGetValue(item, out var value)) { 
                _items[item] = (GetItemBucket(item), value.Subscriber, value.CollectionSubscriber);
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item));
            return bucket;
        }
        /// <summary>
        /// Adds or updates an item in the deduplicator. If the item is already in the deduplicator it will be removed from its old bucket and added to the new bucket.
        /// </summary>
        /// <param name="item">The item to update</param>
        public virtual int Set(T item)
        {
            if(_items.TryGetValue(item, out var value))
            {
                return Update(value.Bucket, item);
            }
            else
            {
                return Add(item);
            }
        }
        /// <inheritdoc/>
        public override void Remove(T item)
        {
            base.Remove(item);

            if(_items.TryGetValue(item, out var value))
            {
                Unsubscribe(item, value);
                _items.Remove(item);
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        }

        /// <summary>
        /// Subscribes to the <see cref="CollectionChanged"/> event of the given <paramref name="observable"/> and adds/removes items from the deduplicator when the collection changes.
        /// </summary>
        /// <param name="observable">The observable to subscribe to.</param>
        /// <param name="propertyName">When <see cref="Deduplicator{T}.By"/> is set, the collection can optionally be watched for changes on that property if it implements <see cref="INotifyPropertyChanged"/></param>
        public void Subscribe(INotifyCollectionChanged observable, string propertyName = null)
        {
            observable = observable.ValidateArgument(nameof(observable));
            if(_watched == null)
            {
                _watched = new List<NotifyCollectionChangedEventHandler>();
            }
            var subscriber = new NotifyCollectionChangedEventHandler((sender, args) =>
            {
                switch (args.Action) { 
                    case NotifyCollectionChangedAction.Replace:
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in args.NewItems)
                        {
                            if(item is T tItem)
                            {
                                Set(tItem);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in args.OldItems)
                        {
                            if(item is T tItem)
                            {
                                Remove(tItem);
                            }
                        }
                        break;

                }
            });
            observable.CollectionChanged += subscriber;
            _watched.Add(subscriber);

            if(By != null && propertyName != null && observable is INotifyPropertyChanged notifyPropertyChanged)
            {
                if(_propertyWatched == null)
                {
                    _propertyWatched = new List<PropertyChangedEventHandler>();
                }
                var propertySubscriber = new PropertyChangedEventHandler((sender, args) =>
                {
                    if(sender is T tItem && args.PropertyName == propertyName)
                    {
                        Set(tItem);
                    }
                });
                notifyPropertyChanged.PropertyChanged += propertySubscriber;
            }
        }
        /// <summary>
        /// Unsubscribes from the <see cref="CollectionChanged"/> event of the given <paramref name="observable"/>.
        /// </summary>
        /// <param name="observable">The observable to unsubscribe from.</param>
        public void Unsubscribe(INotifyCollectionChanged observable)
        {
            observable = observable.ValidateArgument(nameof(observable));
            if(_watched == null)
            {
                return;
            }
            foreach (var subscriber in _watched)
            {
                observable.CollectionChanged -= subscriber;
            }
            if(_propertyWatched != null && observable is INotifyPropertyChanged notifyPropertyChanged)
            {
                foreach (var propertySubscriber in _propertyWatched)
                {
                    notifyPropertyChanged.PropertyChanged -= propertySubscriber;
                }
            }
        }

        private void Unsubscribe(T item, (int Bucket, PropertyChangedEventHandler Subscriber, NotifyCollectionChangedEventHandler CollectionSubscriber) value)
        {
            if (item is INotifyPropertyChanged notifyPropertyChanged && value.Subscriber != null)
            {
                notifyPropertyChanged.PropertyChanged -= value.Subscriber;
            }
            if (item is INotifyCollectionChanged notifyCollectionChanged && value.CollectionSubscriber != null)
            {
                notifyCollectionChanged.CollectionChanged -= value.CollectionSubscriber;
            }
        }
    }
}
