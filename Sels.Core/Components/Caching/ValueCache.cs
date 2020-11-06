using Sels.Core.Extensions.General.Generic;
using Sels.Core.Extensions.General.Validation;
using Sels.Core.Extensions.Object;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Caching
{
    public class ValueCache<T>
    {
        // Fields
        private readonly object _threadLock = new object();

        private T _internalValue;

        private readonly Predicate<T> _setCase = DefaultSetCase;
        private readonly Func<T> _valueInitializer;

        // Properties 
        public T Value { 
            get {
                lock (_threadLock)
                {
                    if (_setCase(_internalValue))
                    {
                        _internalValue = _valueInitializer();
                    }
                }

                return _internalValue;
            }
        }

        public ValueCache(T value, Func<T> valueInitializer)
        {
            valueInitializer.ValidateVariable(nameof(valueInitializer));

            _valueInitializer = valueInitializer;
            Set(value);
        }

        public ValueCache(Func<T> valueInitializer)
        {
            valueInitializer.ValidateVariable(nameof(valueInitializer));

            _valueInitializer = valueInitializer;
        }

        public ValueCache(T value, Predicate<T> setCase, Func<T> valueInitializer)
        {
            valueInitializer.ValidateVariable(nameof(valueInitializer));
            setCase.ValidateVariable(nameof(setCase));

            _valueInitializer = valueInitializer;
            _setCase = setCase;
            Set(value);
        }

        public ValueCache(Predicate<T> setCase, Func<T> valueInitializer)
        {
            valueInitializer.ValidateVariable(nameof(valueInitializer));
            setCase.ValidateVariable(nameof(setCase));

            _valueInitializer = valueInitializer;
            _setCase = setCase;
        }


        public void Set(T value)
        {
            lock (_threadLock)
            {
                _internalValue = value;
            }
        }

        public void ResetCache()
        {
            lock (_threadLock)
            {
                _internalValue = default;
            }
        }


        private static bool DefaultSetCase(T value)
        {
            return value.IsDefault();
        }
    }
}
