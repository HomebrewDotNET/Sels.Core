using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Serialization
{
    public interface ISerializationProvider
    {
        string Serialize<T>(T value);

        T Deserialize<T>(string value) where T : new();
    }
}
