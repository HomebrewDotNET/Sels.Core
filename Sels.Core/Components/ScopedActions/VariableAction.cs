using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Variable
{
    public class VariableAction<T> : IDisposable
    {
        // Fields
        private readonly Action<T> _variableSetter;
        private readonly Func<T> _variableAction;

        public VariableAction(Action<T> variableSetter, Func<T> variableAction)
        {
            variableSetter.ValidateVariable(nameof(variableSetter));
            variableAction.ValidateVariable(nameof(variableAction));

            _variableSetter = variableSetter;
            _variableAction = variableAction;
        }

        public VariableAction(T initialValue, Action<T> variableSetter, Func<T> variableAction)
        {
            variableSetter.ValidateVariable(nameof(variableSetter));
            variableAction.ValidateVariable(nameof(variableAction));

            _variableSetter = variableSetter;
            _variableAction = variableAction;

            variableSetter(initialValue);
        }

        public void Dispose()
        {
            _variableSetter(_variableAction());
        }
    }
}
