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
    public class Parameterizer
    {
        // Constants
        public const string ArgumentSplit = "_";

        // Fields
        private readonly Dictionary<string, Parameter> _parameters = new Dictionary<string, Parameter>();

        // Properties
        public ReadOnlyCollection<Parameter> Parameters => new ReadOnlyCollection<Parameter>(_parameters.Select(x => x.Value).ToList());

        public static Property<string> ParameterPrefix { get; } = new Property<string>().AddGetterSetCondition(x => !x.HasValue()).AddGetterSetter(() => GlobalParameters.Prefix.Value).AddSetterValidation(x => x.HasValue(), $"{nameof(ParameterPrefix)} cannot be null, empty or whitespace");

        public static Property<string> ParameterSuffix { get; } = new Property<string>().AddGetterSetCondition(x => !x.HasValue()).AddGetterSetter(() => GlobalParameters.Suffix.Value).AddSetterValidation(x => x.HasValue(), $"{nameof(ParameterSuffix)} cannot be null, empty or whitespace");

        public Parameterizer(bool loadGlobalParameters = true)
        {
            if (loadGlobalParameters)
            {
                AddParameters(GlobalParameters.Parameters);
            }
        }

        #region AddParameter
        /// <summary>
        /// Adds parameter that the parameterizer can use
        /// </summary>
        public Parameterizer AddParameter(string name, Func<object, string, string> generateValueAction, Func<object> beginScopeAction = null)
        {
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));
            generateValueAction.ValidateArgument(nameof(generateValueAction));

            return AddParameter(new DelegateParameter(name, generateValueAction, beginScopeAction));
        }

        /// <summary>
        /// Adds parameter that the parameterizer can use
        /// </summary>
        public Parameterizer AddParameter(string name, Func<string> generateValueAction, Func<object> beginScopeAction = null)
        {
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));
            generateValueAction.ValidateArgument(nameof(generateValueAction));

            return AddParameter(name, (x, y) => generateValueAction(), beginScopeAction);
        }

        /// <summary>
        /// Adds parameter that the parameterizer can use
        /// </summary>
        public Parameterizer AddParameter(string name, string parameterValue)
        {
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            return AddParameter(name, (x,y) => parameterValue);
        }

        /// <summary>
        /// Adds parameter that the parameterizer can use
        /// </summary>
        public Parameterizer AddParameter(string name, object parameterValue)
        {
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));
            parameterValue.ValidateArgument(nameof(parameterValue));

            return AddParameter(name, parameterValue.ToString());
        }

        /// <summary>
        /// Adds parameter that the parameterizer can use
        /// </summary>
        public Parameterizer AddParameter(Parameter parameter)
        {
            parameter.ValidateArgument(nameof(parameter));
            parameter.Name.ValidateArgumentNotNullOrWhitespace(nameof(parameter.Name));

            _parameters.AddOrUpdate(parameter.Name, parameter);

            return this;
        }
        /// <summary>
        /// Adds multiple parameters that the parameterizer can use
        /// </summary>
        public Parameterizer AddParameters(params Parameter[] parameters)
        {
            parameters.ValidateArgument(nameof(parameters));

            foreach(var parameter in parameters)
            {
                AddParameter(parameter);
            }

            return this;
        }
        /// <summary>
        /// Adds multiple parameters that the parameterizer can use
        /// </summary>
        public Parameterizer AddParameters(IEnumerable<Parameter> parameters)
        {
            parameters.ValidateArgument(nameof(parameters));

            foreach (var parameter in parameters)
            {
                AddParameter(parameter);
            }

            return this;
        }
        #endregion

        #region RemoveParameter
        public void RemoveParameterIfExists(string name)
        {
            name.ValidateVariable(nameof(name));

            if (_parameters.ContainsKey(name))
            {
                _parameters.Remove(name);
            }
        }
        #endregion

        /// <summary>
        /// Applies any parameters that are registered in this parameterizer to the supplied text
        /// </summary>
        public string Apply(string source)
        {
            if (source.HasValue())
            {
                var builder = new StringBuilder(source);
                var scopedParameters = _parameters.Select(x => (Scope: x.Value.BeginScope(), Parameter: x.Value));
                var modified = true;

                while (modified)
                {
                    var sourceText = builder.ToString();
                    modified = false;

                    // Loop over parameters and see if sourceText contains a parameter
                    foreach(var scopedParameter in scopedParameters)
                    {
                        var scope = scopedParameter.Scope;
                        var parameter = scopedParameter.Parameter;

                        if(TryFindNextParameter(sourceText, parameter.Name, out string fullParameter, out string argument))
                        {
                            // We found a parameter so we generate a new value using the argument and replace the parameter in the source string
                            var paramaterValue = parameter.GenerateNewValue(scope, argument);
                            builder.Replace(fullParameter, paramaterValue);
                            modified = true;
                        }
                    }                   
                }

                return builder.ToString();
            }
            else
            {
                return source;
            }
        }

        private bool TryFindNextParameter(string sourceText, string parameterName, out string fullParameter, out string argument)
        {
            sourceText.ValidateVariable(nameof(sourceText));
            parameterName.ValidateVariable(nameof(parameterName));
            fullParameter = null;
            argument = null;

            var parameterStart = ParameterPrefix.Value + parameterName;
            var currentStartIndex = sourceText.IndexOf(parameterStart);

            while (true)
            {
                if(currentStartIndex >= 0)
                {
                    // Find next parameter start
                    var nextStartIndex = sourceText.IndexOf(ParameterPrefix.Value, currentStartIndex + 1);
                    // Find next parameter end
                    var nextEndIndex = sourceText.IndexOf(ParameterSuffix.Value, currentStartIndex + 1);
                   
                    if(nextEndIndex >= 0)
                    {
                        // If we have a next parameter start and it falls before the next parameter end we have a sub parameter so we skip otherwise we found a parameter when we have a next parameter end
                        if (nextStartIndex < 1 || nextStartIndex > nextEndIndex)
                        {
                            fullParameter = sourceText.Substring(currentStartIndex, nextEndIndex - currentStartIndex + ParameterSuffix.Value.Length);
                            fullParameter.Substring(ParameterPrefix.Value.Length, fullParameter.Length - ParameterPrefix.Value.Length - ParameterSuffix.Value.Length).TrySplitFirstOrDefault(ArgumentSplit, out argument);
                            return true;
                        }
                        else
                        {
                            currentStartIndex = sourceText.IndexOf(parameterStart, nextEndIndex);
                        }
                        
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
