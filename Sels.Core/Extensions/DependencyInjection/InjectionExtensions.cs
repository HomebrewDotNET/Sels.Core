using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sels.Core.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods for injecing dependencies into objects.
    /// </summary>
    public static class InjectionExtensions
    {
        /// <summary>
        /// Tries to set the values in <paramref name="injectables"/> to the right property in <paramref name="source"/> by comparing the property name and <paramref name="injectables"/> key.
        /// </summary>
        /// <typeparam name="T">Type of object to inject</typeparam>
        /// <param name="source">Object to set properties on</param>
        /// <param name="injectables">Values to set on <paramref name="source"/></param>
        /// <returns><paramref name="source"/></returns>
        public static T InjectProperties<T>(this T source, IDictionary<string, object> injectables)
        {
            source.ValidateArgument(nameof(source));

            if (injectables.HasValue())
            {
                foreach(var property in source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {                   
                    if (injectables.ContainsKey(property.Name))
                    {
                        var value = injectables[property.Name];
                        var propertyType = property.PropertyType;
                        var injectableType = value.GetTypeOrDefault();

                        if(injectableType.HasValue())
                        {
                            property.SetValue(source, Convert.ChangeType(value, propertyType));
                        }                      
                    }
                }
            }

            return source;
        }
    }
}
