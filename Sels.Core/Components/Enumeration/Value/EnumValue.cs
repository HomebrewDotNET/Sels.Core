using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Enumeration.Value
{
    public class EnumValue : Attribute
    {
        // Properties
        public string Value { get; set; }

        public EnumValue(string enumValue)
        {
            Value = enumValue;
        }
    }
}
