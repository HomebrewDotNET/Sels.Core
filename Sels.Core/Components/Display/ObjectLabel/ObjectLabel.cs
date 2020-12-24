using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Display.ObjectLabel
{
    public class ObjectLabel : Attribute
    {
        public string Label { get; }
        public ObjectLabel(string label)
        {
            Label = label;    
        }
    }
}
