using Sels.Core.Components.Serialization.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Serialization
{
    public enum SerializationProvider
    {
        Json,
        Bson,
        Xml,
        Null
    }

    public static class SerializationProviderExtenions
    {
        public static ISerializationProvider CreateProvider(this SerializationProvider provider)
        {
            switch (provider)
            {
                case SerializationProvider.Bson:
                    return new BsonProvider();
                case SerializationProvider.Json:
                    return new JsonProvider();
                case SerializationProvider.Xml:
                    return new XmlProvider();
            }

            throw new NotSupportedException($"SerializationProvider {provider} is not supported");
        }
    }
}
