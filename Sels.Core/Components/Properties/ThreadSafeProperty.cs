using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Properties
{
    public class ThreadSafeProperty<TValue> : Property<TValue>
    {
        // Fields
        private readonly object _threadLock = new object();

        public ThreadSafeProperty(object threadLock = null) : base()
        {
            _threadLock = threadLock ?? new object();
        }

        public ThreadSafeProperty(TValue initialValue, object threadLock = null) : base(initialValue)
        {
            _threadLock = threadLock ?? new object();
        }

        #region Configuration
        /// <summary>
        /// Adds a Getter Set condition. If the condition returns true the internal value will be set using the getterSetter
        /// </summary>
        public new ThreadSafeProperty<TValue> AddGetterSetCondition(Predicate<TValue> condition)
        {
            condition.ValidateVariable(nameof(condition));

            base.AddGetterSetCondition(condition);

            return this;
        }

        /// <summary>
        /// Returns value for the internal value when any of the Getter Set conditions return true
        /// </summary>
        public new ThreadSafeProperty<TValue> AddGetterSetter(Func<TValue> getterSetter)
        {
            getterSetter.ValidateVariable(nameof(getterSetter));

            base.AddGetterSetter(getterSetter);

            return this;
        }

        /// <summary>
        /// Adds validation on setting the internal value. InvalidOperationException is thrown using the errorMessage when the Condition fails
        /// </summary>
        public new ThreadSafeProperty<TValue> AddSetterValidation(Predicate<TValue> condition, string errorMessage)
        {
            condition.ValidateVariable(nameof(condition));
            errorMessage.ValidateVariable(nameof(errorMessage));

            _setterValidators.Add((condition, () => new InvalidOperationException(errorMessage)));

            return this;
        }

        /// <summary>
        /// Adds validation on setting the internal value. Exception is thrown using the ErrorException Func when the Condition fails
        /// </summary>
        public new ThreadSafeProperty<TValue> AddSetterValidation(Predicate<TValue> condition, Func<Exception> errorException)
        {
            condition.ValidateVariable(nameof(condition));
            errorException.ValidateVariable(nameof(errorException));

            _setterValidators.Add((condition, errorException));

            return this;
        }
        #endregion

        protected override TValue Get()
        {
            lock (_threadLock)
            {
                return base.Get();
            }
        }

        protected override void Set(TValue value)
        {
            lock (_threadLock)
            {
                base.Set(value);
            }
        }
    }
}
