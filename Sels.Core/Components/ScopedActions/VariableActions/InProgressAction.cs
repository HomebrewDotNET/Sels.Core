using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Variable.VariableActions
{
    public class InProgressAction : VariableAction<bool>
    {
        public InProgressAction(Action<bool> variableSetter) : base(true, variableSetter, () => false)
        {

        }
    }
}
