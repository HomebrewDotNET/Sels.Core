using Sels.Core.Extensions.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Serialization
{
    public class JsonProvider : ISerializationProvider
    {
        public T Deserialize<T>(string value)
        {
            return value.DeserializeFromJson<T>();
        }

        public string Serialize<T>(T value)
        {
            return value.SerializeAsJson();
        }
    }
}
