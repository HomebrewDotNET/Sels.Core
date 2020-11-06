using Sels.Core.Components.Caching.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Caching
{
    public class InMemoryCache<T>
    {
        private readonly Dictionary<T, object> _cache;
        private readonly object _threadLock;

        public InMemoryCache()
        {
            _cache = new Dictionary<T, object>();
            _threadLock = new object();
        }

        public void Add(T key, object value)
        {
            lock (_threadLock)
            {
                if(!_cache.ContainsKey(key))
                {
                    _cache.Add(key, value);
                }
                else
                {
                    throw new AlreadyCachedException(key);
                }
            }
        }

        public bool TryAdd(T key, object value)
        {
            lock (_threadLock)
            {
                if (!_cache.ContainsKey(key))
                {
                    _cache.Add(key, value);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Update(T key, object value)
        {
            lock (_threadLock)
            {
                if(_cache.ContainsKey(key))
                {
                    _cache[key] = value;
                }
                else
                {
                    throw new NotCachedException(key);
                }
            }
        }

        public void Persist(T key, object value)
        {
            lock (_threadLock)
            {
                if(!_cache.ContainsKey(key))
                {
                    Add(key, value);
                }
                else
                {
                    Update(key, value);
                }
            }
        }

        public bool Contains(T key)
        {
            lock (_threadLock)
            {
                return _cache.ContainsKey(key);
            }
        }

        public TValue Get<TValue>(T key)
        {
            lock (_threadLock)
            {
                if(_cache.ContainsKey(key))
                {
                    return (TValue)_cache[key];
                }
                else
                {
                    throw new NotCachedException(key);
                }
            }
        }

        public void Clear(T key)
        {
            lock (_threadLock)
            {
                if(_cache.ContainsKey(key))
                {
                    _cache.Remove(key);
                }
                else
                {
                    throw new NotCachedException(key);
                }
            }
        }

        public void ClearAll()
        {
            lock (_threadLock)
            {
                _cache.Clear();
            }
        }
    }
}
