using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Properties
{
    public class ThreadSafeReadOnlyProperty<TValue> : ReadOnlyProperty<TValue>
    {
        // Fields
        private readonly object _threadLock = new object();

        public ThreadSafeReadOnlyProperty(object threadLock = null) : base()
        {
            _threadLock = threadLock ?? new object();
        }

        public ThreadSafeReadOnlyProperty(TValue initialValue, object threadLock = null) : base(initialValue)
        {
            _threadLock = threadLock ?? new object();
        }

        #region Configuration
        /// <summary>
        /// Adds a Getter Set condition. If the condition returns true the internal value will be set using the getterSetter
        /// </summary>
        public new ThreadSafeReadOnlyProperty<TValue> AddGetterSetCondition(Predicate<TValue> condition)
        {
            condition.ValidateVariable(nameof(condition));

            base.AddGetterSetCondition(condition);

            return this;
        }

        /// <summary>
        /// Returns value for the internal value when any of the Getter Set conditions return true
        /// </summary>
        public new ThreadSafeReadOnlyProperty<TValue> AddGetterSetter(Func<TValue> getterSetter)
        {
            getterSetter.ValidateVariable(nameof(getterSetter));

            base.AddGetterSetter(getterSetter);

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
