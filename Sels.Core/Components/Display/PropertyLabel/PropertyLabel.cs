using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Display.PropertyLabel
{
    public class PropertyLabel : Attribute
    {
        public string Label { get; }
        public PropertyLabel(string label)
        {
            Label = label;    
        }
    }
}
