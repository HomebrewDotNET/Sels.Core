using Sels.Core.Extensions.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Serialization.Providers
{
    public class XmlProvider : ISerializationProvider
    {
        public T Deserialize<T>(string value)
        {
            return value.DeserializeFromXml<T>();
        }

        public string Serialize<T>(T value)
        {
            return value.SerializeAsXml();
        }
    }
}
