using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using System;
using System.Collections.Generic;

namespace Sels.Core.Properties
{
    /// <summary>
    /// Value wrapper where the getter and setter are threadsafe.
    /// </summary>
    /// <typeparam name="TValue">The type of the internal value</typeparam>
    public class ThreadSafeProperty<TValue>
    {
        // Fields
        private object _threadLock;
        private TValue _value;
        private List<(Predicate<TValue> Condition ,Func<TValue, Exception> ErrorContructor)> _validators = new List<(Predicate<TValue> Condition, Func<TValue, Exception> ErrorContructor)>();
        private Func<TValue, TValue> _getter = x => x;
        private Func<TValue, TValue> _setter = x => x;

        // Properties
        /// <summary>
        /// Property to get/set the internal value.
        /// </summary>
        public TValue Value { 
            get {
                lock (_threadLock)
                {
                    return _getter(_value);
                }
            } 
            set {
                lock (_threadLock)
                {
                    // Trigger validations
                    _validators.Execute((i, x) =>
                    {
                        if (x.Condition(value))
                        {
                            var exception = x.ErrorContructor(value);

                            if (exception != null) throw exception;
                            throw new InvalidOperationException($"Validator {i} returned null instead of an exception");
                        }
                    });

                    _value = _setter(value);
                }
            } 
        }

        /// <inheritdoc cref="ThreadSafeProperty{TValue}"/>
        /// <param name="initialValue">The initial value to use for <see cref="Value"/></param>
        /// <param name="threadLock">The object to use for thread locking. When null a new object will be used</param>
        public ThreadSafeProperty(TValue initialValue = default, object threadLock = null)
        {
            _value = initialValue;
            _threadLock = threadLock != null ? threadLock : new object();
        }

        #region Configuration
        /// <summary>
        /// Defines a custom getter triggered when calling the getter on <see cref="Value"/>.
        /// </summary>
        /// <param name="getter">The delegate that will be used as getter. The arg is the internally stored value for <see cref="Value"/></param>
        /// <returns>Current property for method chaining</returns>
        public ThreadSafeProperty<TValue> UseGetter(Func<TValue, TValue> getter)
        {
            getter.ValidateArgument(nameof(getter));

            _getter = getter;

            return this;
        }
        /// <summary>
        /// Defines a custom setter triggered when calling the setter on <see cref="Value"/>.
        /// </summary>
        /// <param name="setter">The delegate that will be used as setter. The arg is the value to set for <see cref="Value"/></param>
        /// <returns>Current property for method chaining</returns>
        public ThreadSafeProperty<TValue> UseSetter(Func<TValue, TValue> setter)
        {
            setter.ValidateArgument(nameof(setter));

            _setter = setter;

            return this;
        }
        /// <summary>
        /// Defines delegates for validating the value before setting the internal value.
        /// </summary>
        /// <param name="condition">The delegate that checks if the value is valid</param>
        /// <param name="exceptionBuilder">The delegate that will create the exception to throw when the value isn't valid</param>
        /// <returns>Current property for method chaining</returns>
        public ThreadSafeProperty<TValue> ValidIf(Predicate<TValue> condition, Func<TValue, Exception> exceptionBuilder)
        {
            condition.ValidateArgument(nameof(condition));
            exceptionBuilder.ValidateArgument(nameof(exceptionBuilder));

            _validators.Add((condition, exceptionBuilder));

            return this;
        }
        /// <summary>
        /// Defines delegates for validating the value before setting the internal value.
        /// </summary>
        /// <param name="condition">The delegate that checks if the value is valid</param>
        /// <param name="messageBuilder">The delegate that will create the error message for the <see cref="ArgumentException"/> to throw when the value isn't valid</param>
        /// <returns>Current property for method chaining</returns>
        public ThreadSafeProperty<TValue> ValidIf(Predicate<TValue> condition, Func<TValue, string> messageBuilder)
        {
            condition.ValidateArgument(nameof(condition));
            messageBuilder.ValidateArgument(nameof(messageBuilder));

            return ValidIf(condition, x => new ArgumentException(messageBuilder(x)));
        }
        #endregion
        /// <summary>
        /// Executes <paramref name="action"/> using the internal lock so <paramref name="action"/> can be executed atomically without <see cref="Value"/> being modified.
        /// </summary>
        /// <param name="action">The action to execute</param>
        public void Execute(Action<TValue> action)
        {
            action.ValidateArgument(nameof(action));

            lock (_threadLock)
            {
                action(Value);
            }
        }
        /// <summary>
        /// Executes <paramref name="function"/> using the internal lock so <paramref name="function"/> can be executed atomically without <see cref="Value"/> being modified.
        /// </summary>
        /// <typeparam name="T">Type of object returned from <paramref name="function"/></typeparam>
        /// <param name="function">The function to execute</param>
        /// <returns>The value returned from <paramref name="function"/></returns>
        public T Execute<T>(Func<TValue, T> function)
        {
            function.ValidateArgument(nameof(function));

            lock (_threadLock)
            {
                return function(Value);
            }
        }
    }
}
