using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Extensions.Linq;

namespace Sels.Core.Wpf.Components.Property
{
    public abstract class BasePropertyChangedNotifier : INotifyPropertyChanged
    {
        // Fields
        private readonly Dictionary<PropertyInfo, object> _propertyValues = new Dictionary<PropertyInfo, object>();
        private readonly Dictionary<PropertyInfo, List<Action<bool, object>>> _propertySubscribers = new Dictionary<PropertyInfo, List<Action<bool, object>>>();
        private readonly List<Action<bool, PropertyInfo, object>> _fullPropertySubscribers = new List<Action<bool, PropertyInfo, object>>();

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void RaisePropertyChanged(PropertyInfo propertyThatChanged)
        {
            propertyThatChanged.ValidateVariable(nameof(propertyThatChanged));

            if (PropertyChanged.GetInvocationList().HasValue())
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyThatChanged.Name));
            }
        }

        public void RaisePropertyChanged(string propertyThatChanged)
        {
            propertyThatChanged.ValidateVariable(nameof(propertyThatChanged));

            RaisePropertyChanged(this.GetPropertyInfo(propertyThatChanged));
        }
        #endregion

        #region GetAndSet
        protected T GetValue<T>(PropertyInfo sourceProperty)
        {
            sourceProperty.ValidateVariable(nameof(sourceProperty));

            if (sourceProperty.CanAssign<T>())
            {
                return (T)_propertyValues.TryGetOrSet(sourceProperty, () => default(T));
            }
            else
            {
                throw new InvalidOperationException($"Could not get value on property <{sourceProperty.Name}> because value type <{typeof(T)}> is not assignable from <{sourceProperty.PropertyType}>");
            }
        }
        protected T GetValue<T>(string propertyName)
        {
            propertyName.ValidateVariable(nameof(propertyName));

            return GetValue<T>(this.GetPropertyInfo(propertyName));
        }

        //protected T GetValue<T>(string propertyName, Func<T> customGetter, Action<bool, T> whenChangedAction = null)
        //{
        //    propertyName.ValidateVariable(nameof(propertyName));
        //    customGetter.ValidateVariable(nameof(customGetter));

        //    return GetValue(this.GetPropertyInfo(propertyName), customGetter, whenChangedAction);
        //}

        //protected T GetValue<T>(PropertyInfo sourceProperty, Func<T> customGetter, Action<bool, T> whenChangedAction = null)
        //{
        //    sourceProperty.ValidateVariable(nameof(sourceProperty));
        //    customGetter.ValidateVariable(nameof(customGetter));

        //    var getterValue = customGetter();

        //    var previousValue = GetValue<T>(sourceProperty);

        //    var wasChanged = false;
        //    if (getterValue == null || !previousValue.Equals(getterValue))
        //    {
        //        RaisePropertyChanged(sourceProperty);
        //        wasChanged = true;
        //    }

        //    whenChangedAction.InvokeOrDefault(wasChanged, getterValue);
        //    _propertySubscribers.IfContains(sourceProperty, actions => actions.InvokeOrDefault(wasChanged, getterValue));

        //    return getterValue;
        //}

        protected T SetValue<T>(PropertyInfo sourceProperty, T value, Action<bool, T> whenChangedAction, params PropertyInfo[] affectedProperties)
        {
            sourceProperty.ValidateVariable(nameof(sourceProperty));

            if (sourceProperty.CanAssign<T>())
            {
                var propertyValue = sourceProperty.GetValue<T>(this);
                bool wasChanged = false;

                if (propertyValue == null || !propertyValue.Equals(value))
                {
                    _propertyValues.AddOrUpdate(sourceProperty, value);
                    RaisePropertyChanged(sourceProperty);
                    wasChanged = true;

                    affectedProperties.Execute(x => RaisePropertyChanged(x));
                }

                whenChangedAction.InvokeOrDefault(wasChanged, value);
                _propertySubscribers.IfContains(sourceProperty, actions => actions.InvokeOrDefault(wasChanged, value));
                _fullPropertySubscribers.InvokeOrDefault(wasChanged, sourceProperty, value);
            }
            else
            {
                throw new InvalidOperationException($"Could not set value on property <{sourceProperty.Name}> because value type <{typeof(T)}> is not assignable from <{sourceProperty.PropertyType}>");
            }

            return value;
        }

        protected T SetValue<T>(PropertyInfo sourceProperty, T value, Action changedAction, params PropertyInfo[] affectedProperties)
        {
            return SetValue<T>(sourceProperty, value, (x, y) => changedAction(), affectedProperties);
        }

        protected T SetValue<T>(string propertyName, T value, Action<bool, T> whenChangedAction, params string[] affectedProperties)
        {
            propertyName.ValidateVariable(nameof(propertyName));

            return SetValue(this.GetPropertyInfo(propertyName), value, whenChangedAction, affectedProperties.SelectOrDefault(x => this.GetPropertyInfo(x)));
        }

        protected T SetValue<T>(string propertyName, T value, Action changedAction, params string[] affectedProperties)
        {
            return SetValue(this.GetPropertyInfo(propertyName), value, changedAction, affectedProperties.SelectOrDefault(x => this.GetPropertyInfo(x)));
        }
        protected T SetValue<T>(string propertyName, T value, params string[] affectedProperties)
        {
            return SetValue(this.GetPropertyInfo(propertyName), value, () => { }, affectedProperties.SelectOrDefault(x => this.GetPropertyInfo(x)));
        }
        #endregion

        #region WhenChanged
        public void SubscribeToPropertyChanged<T>(string propertyName, Action<bool, T> whenChangedAction)
        {
            propertyName.ValidateVariable(nameof(propertyName));
            whenChangedAction.ValidateVariable(nameof(whenChangedAction));
            SubscribeToPropertyChanged<T>(this.GetPropertyInfo(propertyName), whenChangedAction);
        }

        public void SubscribeToPropertyChanged<T>(PropertyInfo property, Action<bool, T> whenChangedAction)
        {
            property.ValidateVariable(nameof(property));
            whenChangedAction.ValidateVariable(nameof(whenChangedAction));

            _propertySubscribers.AddValueToList(property, (wasDifferent, value) => whenChangedAction(wasDifferent,(T)value));
        }

        public void SubscribeToAllPropertiesChanged(Action<bool, PropertyInfo, object> whenChangedAction)
        {
            whenChangedAction.ValidateVariable(nameof(whenChangedAction));

            _fullPropertySubscribers.Add(whenChangedAction);
        }
        #endregion
    }
}
