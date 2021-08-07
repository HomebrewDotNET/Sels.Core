using Sels.Core.Components.Conversion;
using Sels.Core.Contracts.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sels.Core.Extensions.DependencyInjection
{
    public static class InjectionExtensions
    {
        /// <summary>
        /// Tries to set the values in <paramref name="injectables"/> to the right property in <paramref name="source"/> by comparing the property name and <paramref name="injectables"/> key.
        /// </summary>
        /// <typeparam name="T">Type of object to inject</typeparam>
        /// <param name="source">Object to set properties on</param>
        /// <param name="injectables">Values to set on <paramref name="source"/></param>
        /// <param name="typeConverter">Converter that converts the values in <paramref name="injectables"/> to the right property type</param>
        /// <returns><paramref name="source"/></returns>
        public static T InjectProperties<T>(this T source, Dictionary<string, object> injectables, IGenericTypeConverter typeConverter)
        {
            source.ValidateArgument(nameof(source));
            typeConverter.ValidateArgument(nameof(typeConverter));

            if (injectables.HasValue())
            {
                foreach(var property in source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {                   
                    if (injectables.ContainsKey(property.Name))
                    {
                        var value = injectables[property.Name];
                        var propertyType = property.PropertyType;
                        var injectableType = value.GetTypeOrDefault();

                        if(injectableType.HasValue() && typeConverter.CanConvert(injectableType, propertyType, value))
                        {
                            property.SetValue(source, typeConverter.ConvertTo(injectableType, propertyType, value));
                        }                      
                    }
                }
            }

            return source;
        }

        /// <summary>
        /// Tries to set the values in <paramref name="injectables"/> to the right property in <paramref name="source"/> by comparing the property name and <paramref name="injectables"/> key. <see cref="GenericConverter.DefaultConverter"/> will be used as converter.
        /// </summary>
        /// <typeparam name="T">Type of object to inject</typeparam>
        /// <param name="source">Object to set properties on</param>
        /// <param name="injectables">Values to set on <paramref name="source"/></param>
        /// <returns><paramref name="source"/></returns>
        public static T InjectProperties<T>(this T source, Dictionary<string, object> injectables)
        {
            source.ValidateArgument(nameof(source));

            return source.InjectProperties(injectables, GenericConverter.DefaultConverter);
        }
    }
}
