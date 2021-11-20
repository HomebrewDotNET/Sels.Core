using Sels.Core.Contracts.Serialization.KeyValue;
using Sels.Core.Extensions.Conversion;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sels.Core.Extensions.Reflection;
using System.Collections;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Execution;

namespace Sels.Core.Components.Serialization.KeyValue.Converters
{
    public class CollectionFactory : IKeyValueCollectionFactory
    {
        public object CreateCollection(Type collectionType, IEnumerable<object> deserializedItems, object factoryArgument = null)
        {
            if (collectionType.IsArray)
            {
                // Create new array instance
                var array = Array.CreateInstance(collectionType.GetItemTypeFromContainer(), deserializedItems.Count());

                var index = 0;

                // Add items to array
                foreach(var item in deserializedItems)
                {
                    array.SetValue(item, index);

                    index++;
                }

                return array;
            }
            else
            {
                if(collectionType.IsAssignableTo<IList>())
                {
                    var list = collectionType.Construct<IList>();
                    deserializedItems.Where(x => x.HasValue()).Execute(x => list.Add(x));

                    return list;
                }
                else if (collectionType.CanConstructWithArguments(deserializedItems))
                {
                    return collectionType.Construct(deserializedItems);
                }
            }

            return collectionType.GetDefaultValue();
        }

        public string[] ParseItems(IEnumerable<string> serializedItems, object factoryArgument = null)
        {
            return serializedItems.ToArray();
        }
    }
}
