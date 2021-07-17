using Sels.Core.Contracts.Serialization.KeyValue;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Serialization.KeyValue.Attributes
{
    public abstract class BaseKeyValueAttribute : Attribute, IKeyValueSerializationAttribute
    {
        // Properties
        public string Key { get; }
        public bool Ignore { get; }

        public BaseKeyValueAttribute(string key = null, bool ignore = false)
        {
            Key = key;
            Ignore = ignore;
        }

        public abstract object Deserialize(Type propertyType, IEnumerable<string> values, object propertyValue = null);

        public abstract string[] Serialize(Type propertyType, object propertyValue);
    }
}
