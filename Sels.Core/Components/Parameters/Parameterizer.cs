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
    /// Service that replaces text parameters in a string using parameters who can perform custom code when resolving a parameter value.
    /// </summary>
    public class Parameterizer
    {
        // Constants
        /// <summary>
        /// The string used to split up a parameter name and it's optional argument.
        /// </summary>
        public const string ArgumentSplit = "_";

        // Fields
        private readonly Dictionary<string, Parameter> _parameters = new Dictionary<string, Parameter>();

        // Properties
        /// <summary>
        /// The parameters that this service will use to resolve parameters in the supplied text.
        /// </summary>
        public ReadOnlyCollection<Parameter> Parameters => new ReadOnlyCollection<Parameter>(_parameters.Select(x => x.Value).ToList());

        /// <summary>
        /// The prefix added in front of the parameters.
        /// </summary>
        public static Property<string> ParameterPrefix { get; } = new Property<string>().AddGetterSetCondition(x => !x.HasValue()).AddGetterSetter(() => GlobalParameters.Prefix.Value).AddSetterValidation(x => x.HasValue(), $"{nameof(ParameterPrefix)} cannot be null, empty or whitespace");
        /// <summary>
        /// The suffix added to close a parameter.
        /// </summary>
        public static Property<string> ParameterSuffix { get; } = new Property<string>().AddGetterSetCondition(x => !x.HasValue()).AddGetterSetter(() => GlobalParameters.Suffix.Value).AddSetterValidation(x => x.HasValue(), $"{nameof(ParameterSuffix)} cannot be null, empty or whitespace");

        /// <summary>
        /// Service that replaces text parameters in a string using parameters who can perform custom code when resolving a parameter value.
        /// </summary>
        /// <param name="loadGlobalParameters">If this service should add the parameters from <see cref="GlobalParameters.Parameters"/></param>
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
        /// Replaces all parameters in <paramref name="source"/> that this service knows.
        /// </summary>
        /// <param name="source">The string to replace the parameters in</param>
        /// <returns><paramref name="source"/> will all known parameters replaced</returns>
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

                        var matches = FindParameters(sourceText, parameter.Name);
                        foreach(var match in matches)
                        {
                            modified = true;
                            // Ignore if argument contains a parameter
                            if(match.Argument.HasValue() && _parameters.Any(x => ContainsParameter(match.Argument, x.Key)))
                            {
                                continue;
                            }

                            // We found a parameter so we generate a new value using the argument and replace the parameter in the source string
                            var paramaterValue = parameter.GenerateNewValue(scope, match.Argument);
                            builder.Replace(match.FullParameter, paramaterValue);
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

        private (string FullParameter, string Argument)[] FindParameters(string sourceText, string parameterName, int maxMatches = 0)
        {
            sourceText.ValidateArgument(nameof(sourceText));
            parameterName.ValidateArgument(nameof(parameterName));
            var matches = new List<(string FullParameter, string Argument)>();

            var parameterStart = ParameterPrefix.Value + parameterName;
            var currentIndex = 0;

            while (true)
            {
                // Find next occurance
                var indexOfParameter = sourceText.IndexOf(parameterStart, currentIndex);

                // No start index found so we return
                if (indexOfParameter == -1) break;

                // Search for the parameter suffix for the current parameter taking into account sub parameters
                var currentLevel = 1;
                var currentEndIndex = indexOfParameter+1;
                int indexOfParameterEnd = 0;
                while (true)
                {
                    var nextPrefixIndex = sourceText.IndexOf(ParameterPrefix.Value, currentEndIndex);
                    var nextSuffixIndex = sourceText.IndexOf(ParameterSuffix.Value, currentEndIndex);

                    // No end parameter found so we return
                    if (nextSuffixIndex == -1) matches.ToArray();

                    // We have the start of a parameter before the next end so we have a sub parameter.
                    if (nextPrefixIndex != -1 && nextPrefixIndex < nextSuffixIndex)
                    {
                        currentLevel++;
                        currentEndIndex = nextPrefixIndex + ParameterPrefix.Value.Length;
                    }
                    // We have the end of a parameter before the next start which means the previous parameter was closed.
                    else
                    {
                        currentLevel--;
                        currentEndIndex = nextSuffixIndex + ParameterSuffix.Value.Length;
                    }

                    // We found the parameter suffix for the current parameter.
                    if (currentLevel == 0)
                    {
                        indexOfParameterEnd = nextSuffixIndex;
                        break;
                    }
                }

                var fullParameter = sourceText.Substring(indexOfParameter, (indexOfParameterEnd-indexOfParameter)+ParameterSuffix.Value.Length);
                fullParameter.Substring(ParameterPrefix.Value.Length, fullParameter.Length - ParameterPrefix.Value.Length - ParameterSuffix.Value.Length).TrySplitFirstOrDefault(ArgumentSplit, out var argument);

                matches.Add((fullParameter, argument));

                // Return if we have the required amount of matches
                if (maxMatches != 0 && matches.Count == maxMatches) return matches.ToArray();

                currentIndex = indexOfParameter + ParameterPrefix.Value.Length;
            }

            
            return matches.ToArray();
        }

        private bool ContainsParameter(string sourceText, string parameterName)
        {
            return FindParameters(sourceText, parameterName, 1).HasValue();
        }
    }
}
