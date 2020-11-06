using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.RecurringAction
{
    internal class IdentifierNotUniqueException<T> : Exception
    {

        public IdentifierNotUniqueException(T identifier) : base($"Identifier {identifier.ToString()} is not unique")
        {

        }
    }
}
