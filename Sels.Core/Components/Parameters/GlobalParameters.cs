using Sels.Core.Components.Parameters.Parameters;
using Sels.Core.Components.Properties;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Sels.Core.Components.Parameters
{
    /// <summary>
    /// User to modify the default values used by <see cref="Parameterizer"/>.
    /// </summary>
    public static class GlobalParameters
    {
        // Constants
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public const string DefaultParameterPrefix = "${{";
        public const string DefaultParameterSuffix = "}}";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        // Fields
        private readonly static object _threadLock = new object();
        private readonly static List<Parameter> _defaultGlobalParameters = new List<Parameter>() { 
            new DateTimeNowParameter(),
            new NewGuidParameter()
        };
        private readonly static Dictionary<string, Parameter> _globalParameters = new Dictionary<string, Parameter>();

        // Properties
        public static ReadOnlyCollection<Parameter> Parameters { 
            get {
                lock (_threadLock)
                {
                    return new ReadOnlyCollection<Parameter>(_globalParameters.Select(x => x.Value).ToList());
                }
            }
        }
        public static ThreadSafeProperty<string> Prefix { get; } = new ThreadSafeProperty<string>().UseGetter(x => x.HasValue() ? x : DefaultParameterPrefix).ValidIf(x => x.HasValue(), x => $"{nameof(Prefix)} cannot be null, empty or whitespace");

        public static ThreadSafeProperty<string> Suffix { get; } = new ThreadSafeProperty<string>().UseGetter(x => x.HasValue() ? x : DefaultParameterSuffix).ValidIf(x => x.HasValue(), x => $"{nameof(Suffix)} cannot be null, empty or whitespace");

        static GlobalParameters()
        {
            LoadDefaults();
        }

        #region AddParameter
        /// <summary>
        /// Adds a global parameter that gets loaded into any Parameterizer that has loadGlobalParameters enabled
        /// </summary>
        public static void AddParameter(string name, Func<object, string, string> generateValueAction, Func<object> beginScopeAction = null)
        {
            name.ValidateArgument(nameof(name));
            generateValueAction.ValidateArgument(nameof(generateValueAction));

            AddParameter(new DelegateParameter(name, generateValueAction, beginScopeAction));
        }

        public static void AddParameter(string name, Func<string> generateValueAction, Func<object> beginScopeAction = null)
        {
            name.ValidateArgument(nameof(name));
            generateValueAction.ValidateArgument(nameof(generateValueAction));

            AddParameter(name, (x, y) => generateValueAction(), beginScopeAction);
        }

        /// <summary>
        /// Adds a global parameter that gets loaded into any Parameterizer that has loadGlobalParameters enabled
        /// </summary>
        public static void AddParameter(string name, string parameterValue)
        {
            name.ValidateArgument(nameof(name));

            AddParameter(name, (x, y) => parameterValue);
        }

        /// <summary>
        /// Adds a global parameter that gets loaded into any Parameterizer that has loadGlobalParameters enabled
        /// </summary>
        public static void AddParameter(Parameter parameter)
        {
            parameter.ValidateArgument(nameof(parameter));
            parameter.Name.ValidateArgument(nameof(parameter.Name));

            lock (_threadLock)
            {
                _globalParameters.AddOrUpdate(parameter.Name, parameter);
            }           
        }

        /// <summary>
        /// Adds multiple global parameters that get loaded into any Parameterizer that has loadGlobalParameters enabled
        /// </summary>
        public static void AddParameters(params Parameter[] parameters)
        {
            parameters.ValidateArgument(nameof(parameters));

            foreach (var parameter in parameters)
            {
                AddParameter(parameter);
            }
        }
        /// <summary>
        /// Adds multiple global parameters that get loaded into any Parameterizer that has loadGlobalParameters enabled
        /// </summary>
        public static void AddParameters(IEnumerable<Parameter> parameters)
        {
            parameters.ValidateArgument(nameof(parameters));

            foreach (var parameter in parameters)
            {
                AddParameter(parameter);
            }
        }
        #endregion

        #region RemoveParameter
        public static void RemoveParameterIfExists(string name)
        {
            name.ValidateArgument(nameof(name));

            lock (_threadLock)
            {
                if (_globalParameters.ContainsKey(name))
                {
                    _globalParameters.Remove(name);
                }
            }
        }
        #endregion

        private static void LoadDefaults()
        {
            AddParameters(_defaultGlobalParameters);
        }
    }
}
