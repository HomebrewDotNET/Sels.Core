using Sels.Core.Contracts.Serialization;
using Sels.Core.Extensions.Conversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Serialization.Providers
{
    public class JsonProvider : ISerializationProvider
    {
        public T Deserialize<T>(string value) where T : new()
        {
            return value.DeserializeFromJson<T>();
        }

        public string Serialize<T>(T value)
        {
            return value.SerializeAsJson();
        }
    }
}
