using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Components.Properties
{
    public class ReadOnlyProperty<TValue>
    {
        // Fields
        private TValue _internalValue;

        private Func<TValue> _getterSetter = new Func<TValue>(() => default);
        private readonly List<Predicate<TValue>> _getterSetConditions = new List<Predicate<TValue>>();

        // Properties
        public virtual TValue Value { 
            get {
                return Get();
            } 
            protected set {
                Set(value);
            }
        }

        public ReadOnlyProperty()
        {

        }

        public ReadOnlyProperty(TValue initialValue)
        {
            Set(initialValue);
        }

        #region Configure
        /// <summary>
        /// Adds a Getter Set condition. If the condition returns true the internal value will be set using the getterSetter
        /// </summary>
        public virtual ReadOnlyProperty<TValue> AddGetterSetCondition(Predicate<TValue> condition)
        {
            condition.ValidateVariable(nameof(condition));

            _getterSetConditions.Add(condition);

            return this;
        }

        /// <summary>
        /// Returns value for the internal value when any of the Getter Set conditions return true
        /// </summary>
        public virtual ReadOnlyProperty<TValue> AddGetterSetter(Func<TValue> getterSetter)
        {
            getterSetter.ValidateVariable(nameof(getterSetter));

            _getterSetter = getterSetter;

            return this;
        }
        #endregion

        protected virtual TValue Get()
        {
            // If we have any getter set conditions and at least 1 returns true we set the internal value using the getterSetter or default value of TValue
            if(_getterSetConditions.HasValue(x => x(_internalValue)))
            {
                Set(_getterSetter());
            }

            return _internalValue;
        }

        protected virtual void Set(TValue value)
        {
            _internalValue = value;
        }
    }
}
