using Dapper;
using System.Reflection;

namespace Sels.Core.Data.SQL.Extensions.Dapper
{
    /// <summary>
    /// Contains extension methods for working with dapper.
    /// </summary>
    public static class DapperExtensions
    {
        /// <summary>
        /// Adds the values from the properties on <typeparamref name="T"/> to <paramref name="parameters"/>.
        /// </summary>
        /// <typeparam name="T">Type of instance to add the values from</typeparam>
        /// <param name="parameters">Object to add the values to</param>
        /// <param name="instance">The instance whose values to add to <paramref name="parameters"/></param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns><paramref name="parameters"/> for method chaining</returns>
        public static DynamicParameters AddParametersUsing<T>(this DynamicParameters parameters, T instance, params string[] excludedProperties)
        {
            parameters.ValidateArgument(nameof(parameters));
            instance.ValidateArgument(nameof(instance));

            return parameters.AddParametersUsing(instance, x => x.Name, excludedProperties);
        }

        /// <summary>
        /// Adds the values from the properties on <typeparamref name="T"/> to <paramref name="parameters"/>.
        /// </summary>
        /// <typeparam name="T">Type of instance to add the values from</typeparam>
        /// <param name="parameters">Object to add the values to</param>
        /// <param name="instance">The instance whose values to add to <paramref name="parameters"/></param>
        /// <param name="nameConverter">Delegate for converting the property into a parameter name</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns><paramref name="parameters"/> for method chaining</returns>
        public static DynamicParameters AddParametersUsing<T>(this DynamicParameters parameters, T instance, Func<PropertyInfo, string> nameConverter, params string[] excludedProperties)
        {
            parameters.ValidateArgument(nameof(parameters));
            instance.ValidateArgument(nameof(instance));
            nameConverter.ValidateArgument(nameof(nameConverter));

            var properties = instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetIndexParameters()?.Length == 0 && (!excludedProperties.HasValue() || !excludedProperties.Contains(x.Name, StringComparer.OrdinalIgnoreCase)));

            foreach (var property in properties.Where(x => x.CanRead && x.CanWrite))
            {
                parameters.AddParameter(nameConverter(property), property.GetValue(instance));
            }

            return parameters;
        }

        /// <summary>
        /// Converst <paramref name="instance"/> into dapper parameters using the values from the properties on <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of instance to add the values from</typeparam>
        /// <param name="instance">The instance whose values to add</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>A bag of parameters with the values added from <paramref name="instance"/></returns>
        public static DynamicParameters AsParameters<T>(this T instance, params string[] excludedProperties)
        {
            return new DynamicParameters().AddParametersUsing<T>(instance, excludedProperties);
        }

        /// <summary>
        /// Converst <paramref name="instance"/> into dapper parameters using the values from the properties on <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of instance to add the values from</typeparam>
        /// <param name="instance">The instance whose values to add</param>
        /// <param name="nameConverter">Delegate for converting the property into a parameter name</param>
        /// <param name="excludedProperties">Optional names of properties to exclude</param>
        /// <returns>A bag of parameters with the values added from <paramref name="instance"/></returns>
        public static DynamicParameters AsParameters<T>(this T instance, Func<PropertyInfo, string> nameConverter, params string[] excludedProperties)
        {
            return new DynamicParameters().AddParametersUsing<T>(instance, nameConverter, excludedProperties);
        }

        /// <summary>
        /// Adds a parameter to <paramref name="parameters"/>;
        /// </summary>
        /// <param name="parameters">Object to add the values to</param>
        /// <param name="name">Name of the parameter to add</param>
        /// <param name="value">Value of the parameter to add</param>
        /// <returns><paramref name="parameters"/> for method chaining</returns>
        public static DynamicParameters AddParameter(this DynamicParameters parameters, string name, object value)
        {
            parameters.ValidateArgument(nameof(parameters));
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            // Append prefix if missing
            if (!name.StartsWith(Sql.ParameterPrefix))
            {
                name = Sql.ParameterPrefix + name;
            }

            parameters.Add(name, value);
            return parameters;
        }
    }
}
