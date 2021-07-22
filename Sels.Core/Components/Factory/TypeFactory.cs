using Microsoft.Extensions.Configuration;
using Sels.Core.Components.Conversion;
using Sels.Core.Contracts.Configuration;
using Sels.Core.Contracts.Conversion;
using Sels.Core.Contracts.Factory;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Components.Factory
{
    /// <summary>
    /// Object factory that uses type names as identifier to create new objects. Uses <see cref="ObjectConstructionConfig"/> as the config section.
    /// </summary>
    public class TypeFactory : IObjectFactory
    {
        // Fields
        protected readonly IConfigProvider _configurationProvider;
        protected readonly IGenericTypeConverter _typeConverter;

        public TypeFactory(IConfigProvider configurationProvider, IGenericTypeConverter typeConverter)
        {
            _configurationProvider = configurationProvider.ValidateArgument(nameof(configurationProvider));
            _typeConverter = typeConverter.ValidateArgument(nameof(typeConverter));
        }

        public virtual object Build(string identifier, params object[] arguments)
        {
            return Build<object>(identifier, arguments);
        }

        public virtual T Build<T>(string identifier, params object[] arguments)
        {
            identifier.ValidateArgument(nameof(identifier));

            var type = GetTypeFromIdentifier(nameof(identifier), identifier);

            // Check if type can be constructed with provided arguments
            type.ValidateArgument(x => x.CanConstructWithArguments(arguments), x => new NotSupportedException($"Factory cannot create an object of Type <{type.FullName}> with the supplied arguments"));

            return type.Construct<T>(arguments);
        }

        public virtual object[] BuildAllFromConfig(string section, params string[] parentSections)
        {
            return BuildAllFromConfig<object>(section, parentSections);
        }

        public virtual T[] BuildAllFromConfig<T>(string section, params string[] parentSections)
        {
            section.ValidateArgumentNotNullOrWhitespace(nameof(section));

            var objectConfig = _configurationProvider.GetSectionAs<ObjectConstructionConfig[]>();

            if (objectConfig.HasValue())
            {
                return objectConfig.Select(x => Build<T>(x.Identifier, x.Arguments.Select(y => _typeConverter.ConvertTo(typeof(string), Type.GetType(y.Type), y.Value)).ToArray())).ToArray();
            }

            return default;
        }

        public virtual object BuildFromConfig(string section, params string[] parentSections)
        {
            return BuildFromConfig<object>(section, parentSections);
        }

        public virtual T BuildFromConfig<T>(string section, params string[] parentSections)
        {
            section.ValidateArgumentNotNullOrWhitespace(nameof(section));

            var objectConfig = _configurationProvider.GetSectionAs<ObjectConstructionConfig>();

            return Build<T>(objectConfig.Identifier, objectConfig.Arguments.Select(y => _typeConverter.ConvertTo(typeof(string), Type.GetType(y.Type), y.Value)).ToArray());
        }

        protected virtual Type GetTypeFromIdentifier(string parameterName, string identifier)
        {
            var type = Type.GetType(identifier, false);
            type.ValidateArgument(x => x.HasValue(), $"{parameterName} must be a valid type name");

            return type;
        }
    }
}
