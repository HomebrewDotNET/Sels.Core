﻿using Sels.Core.Contracts.Configuration;
using Sels.Core.Contracts.Conversion;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Components.Factory
{
    /// <summary>
    /// Object factory that uses type aliases as identifier to create new objects. Aliases are loaded from config. Uses <see cref="ObjectConstructionConfig"/> as the config section.
    /// </summary>
    public class AliasTypeFactory : TypeFactory
    {
        // Fields
        protected readonly Dictionary<string, Type> _aliases;

        public AliasTypeFactory(IConfigProvider configurationProvider, IGenericTypeConverter typeConverter, string aliasSection = Constants.Config.Sections.DefaultObjectAliases, params string[] parentSections) : base(configurationProvider, typeConverter)
        {
            Dictionary<string, object> aliases;

            if (parentSections.HasValue())
            {
                Array.Resize(ref parentSections, parentSections.Length + 1);
                parentSections[parentSections.Length - 1] = aliasSection;
                aliases = configurationProvider.GetSectionAs(false, sections: parentSections);
            }
            else
            {
                aliases = configurationProvider.GetSectionAs(aliasSection, false);
            }

            _aliases = (aliases ?? new Dictionary<string, object>()).ToDictionary(x => x.Key, x => Type.GetType(x.Value.ToString()));
        }

        protected override Type GetTypeFromIdentifier(string parameterName, string identifier)
        {
            if (_aliases.ContainsKey(identifier))
            {
                return _aliases[identifier];
            }

            return base.GetTypeFromIdentifier(parameterName, identifier);
        }
    }
}
