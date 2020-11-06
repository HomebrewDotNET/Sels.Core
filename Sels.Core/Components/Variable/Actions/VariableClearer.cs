using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Variable.Actions
{
    public class VariableClearer<T> : VariableAction<T>
    {
        public VariableClearer(Action<T> variableSetter) : base(variableSetter, () => default)
        {

        }

        public VariableClearer(T initialValue, Action<T> variableSetter) : base(initialValue, variableSetter, () => default)
        {

        }
    }
}
