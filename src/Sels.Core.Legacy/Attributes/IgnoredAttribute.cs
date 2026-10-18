using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Attributes
{
    /// <summary>
    /// Attribute that can be put on properties/classes to hint that the property/class should be ignored in certain operations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IgnoredAttribute : Attribute
    {
    }
}
