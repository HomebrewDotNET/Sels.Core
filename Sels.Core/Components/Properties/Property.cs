using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Properties
{
    public class Property<TValue> : ReadOnlyProperty<TValue>
    {
        // Fields
        protected readonly List<(Predicate<TValue> Condition, Func<Exception> ErrorException)> _setterValidators = new List<(Predicate<TValue> Condition, Func<Exception> ErrorException)>();

        // Properties
        public new TValue Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                base.Value = value;
            }
        }

        public Property() : base()
        {

        }

        public Property(TValue initialValue) : base(initialValue)
        {

        }

        #region Configuration
        /// <summary>
        /// Adds a Getter Set condition. If the condition returns true the internal value will be set using the getterSetter
        /// </summary>
        public new Property<TValue> AddGetterSetCondition(Predicate<TValue> condition)
        {
            condition.ValidateVariable(nameof(condition));

            base.AddGetterSetCondition(condition);

            return this;
        }

        /// <summary>
        /// Returns value for the internal value when any of the Getter Set conditions return true
        /// </summary>
        public new Property<TValue> AddGetterSetter(Func<TValue> getterSetter)
        {
            getterSetter.ValidateVariable(nameof(getterSetter));

            base.AddGetterSetter(getterSetter);

            return this;
        }

        /// <summary>
        /// Adds validation on setting the internal value. InvalidOperationException is thrown using the errorMessage when the Condition fails
        /// </summary>
        public Property<TValue> AddSetterValidation(Predicate<TValue> condition, string errorMessage)
        {
            condition.ValidateVariable(nameof(condition));
            errorMessage.ValidateVariable(nameof(errorMessage));

            _setterValidators.Add((condition, () => new InvalidOperationException(errorMessage)));

            return this;
        }

        /// <summary>
        /// Adds validation on setting the internal value. Exception is thrown using the ErrorException Func when the Condition fails
        /// </summary>
        public Property<TValue> AddSetterValidation(Predicate<TValue> condition, Func<Exception> errorException)
        {
            condition.ValidateVariable(nameof(condition));
            errorException.ValidateVariable(nameof(errorException));

            _setterValidators.Add((condition, errorException));

            return this;
        }

        #endregion

        protected override void Set(TValue value)
        {
            if (_setterValidators.HasValue())
            {
                foreach(var (condition, errorException) in _setterValidators)
                {
                    if (!condition(value))
                    {
                        throw errorException();
                    }
                }
            }

            base.Set(value);
        }
    }
}
