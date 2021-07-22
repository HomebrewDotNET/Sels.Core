using Sels.Core.Components.Serialization.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Serialization.Providers
{
    /// <summary>
    /// Common serialization providers.
    /// </summary>
    public enum SerializationProvider
    {
        Json,
        Bson,
        Xml,
        Null
    }
}
