using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace Sels.Core.Entity.Templates
{
    /// <summary>
    /// Base class for objects that need to notify when properties or collections change. Uses <see cref="Metadata"/> to store the values of properties and automatically subscribes to any observables that are set as properties.
    /// </summary>
    public class BaseObservable : INotifyPropertyChanged, INotifyCollectionChanged
    {
        // Fields
        private readonly object _lock = new object();
        private readonly Dictionary<string, (object Obserable, PropertyChangedEventHandler Subscription, NotifyCollectionChangedEventHandler CollectionSubscription)> _subscriptions = new Dictionary<string, (object Obserable, PropertyChangedEventHandler Subscription, NotifyCollectionChangedEventHandler CollectionSubscription)>();
        private Metadata _metadata = new Metadata();

        // Events
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Gets the value for the property with the given <paramref name="propertyName"/> from the metadata.
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The value of the property if it exists, otherwise the default value of <typeparamref name="T"/></returns>
        protected T Get<T>(string propertyName)
        {
            propertyName = propertyName.ValidateArgumentNotNullOrWhitespace(nameof(propertyName));

            if (_metadata.TryGetValue<T>(MetadataKey.Get(propertyName), out var value))
            {
                return value;
            }
            return default;
        }
        /// <summary>
        /// Tries to get the value for the property with the given <paramref name="propertyName"/> from the metadata.
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="value">The value of the property if it exists, otherwise the default value of <typeparamref name="T"/></param>
        /// <returns>True if the property exists, otherwise false</returns>
        protected bool TryGet<T>(string propertyName, out T value)
        {
            propertyName = propertyName.ValidateArgumentNotNullOrWhitespace(nameof(propertyName));
            return _metadata.TryGetValue<T>(MetadataKey.Get(propertyName), out value);
        }
        /// <summary>
        /// Sets the value for the property with the given <paramref name="propertyName"/> in the metadata.
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>True if the property is set, otherwise false</returns>
        protected bool IsSet<T>(string propertyName)
        {
            propertyName = propertyName.ValidateArgumentNotNullOrWhitespace(nameof(propertyName));
            return _metadata.ContainsKey<T>(MetadataKey.Get(propertyName));
        }
        /// <summary>
        /// Gets the value for the property with the given <paramref name="propertyName"/> from the metadata or sets it to the value returned by <paramref name="valueFactory"/> if it does not exist.
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="valueFactory">The function to create the value if it does not exist</param>
        /// <returns>The value of the property</returns>
        protected T GetOrSet<T>(string propertyName, Func<T> valueFactory)
        {
            propertyName = propertyName.ValidateArgumentNotNullOrWhitespace(nameof(propertyName));
            valueFactory = valueFactory.ValidateArgument(nameof(valueFactory));
            lock(_lock)
            {

                var key = MetadataKey.Get(propertyName);
                if(!_metadata.TryGetValue<T>(key, out var value))
                {
                    value = valueFactory();
                    _metadata.Set(key, value);
                }
                return value;
            }
        }
        /// <summary>
        /// Sets the value for the property with the given <paramref name="propertyName"/> in the metadata and raises the <see cref="PropertyChanged"/> event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="value">The value to set</param>
        protected void Set<T>(string propertyName, T value)
        {
            Set<T, T>(propertyName, value, value);
        }

        /// <summary>
        /// Sets the value for the property with the given <paramref name="propertyName"/> in the metadata and raises the <see cref="PropertyChanged"/> event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <typeparam name="TObservable">The type of the observable to subscribe to for property and collection changes</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="value">The value to set</param>
        /// <param name="observable">The observable to subscribe to for property and collection changes</param>
        protected void Set<T, TObservable>(string propertyName, T value, TObservable observable)
        {
            propertyName = propertyName.ValidateArgumentNotNullOrWhitespace(nameof(propertyName));
            lock (_lock)
            {
                var key = MetadataKey.Get(propertyName);
                if (_metadata.TryGetValue<T>(key, out var oldValue))
                {
                    if (EqualityComparer<T>.Default.Equals(oldValue, value))
                    {
                        return;
                    }
                }
                if (_subscriptions.TryGetValue(propertyName, out var subscription))
                {
                    _subscriptions.Remove(propertyName);
                    if (subscription.Subscription != null && subscription.Obserable is INotifyPropertyChanged notifyPropertyChanged)
                    {
                        notifyPropertyChanged.PropertyChanged -= subscription.Subscription;
                    }
                    if (subscription.CollectionSubscription != null && subscription.Obserable is INotifyCollectionChanged notifyCollectionChanged)
                    {
                        notifyCollectionChanged.CollectionChanged -= subscription.CollectionSubscription;
                    }
                }
                if (value == null)
                {
                    _metadata.Unset<T>(key);
                }
                else
                {
                    PropertyChangedEventHandler propertySubscription = null;
                    NotifyCollectionChangedEventHandler collectionSubscription = null;
                    if (observable is INotifyPropertyChanged propertyChanged)
                    {
                        propertySubscription = (sender, args) => PropertyChanged?.Invoke(sender, args);
                        propertyChanged.PropertyChanged += propertySubscription;
                    }
                    if (observable is INotifyCollectionChanged collectionChanged)
                    {
                        collectionSubscription = (sender, args) => CollectionChanged?.Invoke(sender, args);
                        collectionChanged.CollectionChanged += collectionSubscription;
                    }
                    if (propertySubscription != null || collectionSubscription != null)
                    {
                        _subscriptions.Add(propertyName, (observable, propertySubscription, collectionSubscription));
                    }

                    _metadata.Set(key, value);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
