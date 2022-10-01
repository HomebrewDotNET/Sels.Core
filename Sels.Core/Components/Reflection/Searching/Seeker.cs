using Microsoft.Extensions.Logging;
using Sels.Core.Contracts.Reflection.Searching;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sels.Core.Components.Reflection.Searching
{
    /// <summary>
    /// Searches the properties of the supplied object to search for instances of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type to search for</typeparam>
    public class Seeker<T> : ISeeker<T>
    {
        // Fields
        private BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public;
        private readonly List<Predicate<T>> _conditions = new List<Predicate<T>>();
        private readonly List<Func<object, PropertyInfo, object, bool>> _ignoredProperties = new List<Func<object, PropertyInfo, object, bool>>();
        private readonly ILogger[] _loggers;
        private readonly Predicate<object>[] _defaultIgnoredSystemTypes = new Predicate<object>[]
        {
            x => x.GetType().IsString()
        };

        /// <summary>
        /// Searches the properties of the supplied object to search for instances of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="logger">Optional logger for tracing</param>
        /// <param name="ignoreSystemAndMicrosoftTypes">If properties that use microsoft/system types should be ignored for fallthrough</param>
        public Seeker(ILogger logger = null, bool ignoreSystemAndMicrosoftTypes = true) : this(logger.AsArrayOrDefault(), ignoreSystemAndMicrosoftTypes)
        {

        }

        /// <summary>
        /// Searches the properties of the supplied object to search for instances of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="loggers">Optional loggers for tracing</param>
        /// <param name="ignoreSystemAndMicrosoftTypes">If properties that use microsoft/system types should be ignored for fallthrough</param>
        public Seeker(IEnumerable<ILogger> loggers, bool ignoreSystemAndMicrosoftTypes = true)
        {
            _loggers = loggers.ToArrayOrDefault();

            if (ignoreSystemAndMicrosoftTypes) this.IgnoreSystemTypes().IgnoreMicrosoftTypes();
        }
        /// <inheritdoc/>
        public Seeker<T> IgnoreForFallThrough(Func<object, PropertyInfo, object, bool> condition)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                _ignoredProperties.Add(condition);
                return this;
            }
        }
        /// <summary>
        /// Only properties matching <paramref name="flags"/> will be searched.
        /// </summary>
        /// <param name="flags"><inheritdoc cref="BindingFlags"/></param>
        /// <returns>Current instance for method chaining</returns>
        public Seeker<T> SetSearchable(BindingFlags flags)
        {
            using (_loggers.TraceMethod(this))
            {
                _bindingFlags = flags;
                return this;
            }
        }
        /// <inheritdoc/>
        public ISeeker<T> ReturnWhen(params Predicate<T>[] conditions)
        {
            using (_loggers.TraceMethod(this))
            {
                conditions.ValidateArgument(nameof(conditions));

                if (conditions.HasValue())
                {
                    _conditions.AddRange(conditions);
                }
                return this;
            }
        }
        /// <inheritdoc/>
        public IEnumerable<T> SearchAll(object objectToSearch, params object[] additionalObjectsToSearch)
        {
            using (_loggers.TraceMethod(this))
            {
                objectToSearch.ValidateArgument(nameof(objectToSearch));
                _loggers.TraceObject($"Starting search for objects of type <{typeof(T)}> in", objectToSearch);
                _loggers.Debug($"Starting search of <{typeof(T)}> in object of type <{objectToSearch.GetType()}>");
                var context = new SearchContext();
                
                foreach(var item in SearchObject(context, objectToSearch))
                {
                    yield return item;
                }

                if (additionalObjectsToSearch.HasValue())
                {
                    foreach(var otherObjectToSearch in additionalObjectsToSearch.Where(x => x != null))
                    {
                        _loggers.TraceObject($"Starting search for objects of type <{typeof(T)}> in", otherObjectToSearch);
                        _loggers.Debug($"Starting search of <{typeof(T)}> in object of type <{otherObjectToSearch.GetType()}>");

                        foreach (var found in SearchObject(context, otherObjectToSearch))
                        {
                            yield return found;
                        }
                    }
                }
            }
        }

        private IEnumerable<T> SearchObject(SearchContext context, object objectToSearch)
        {
            using (_loggers.TraceMethod(this))
            {
                if(objectToSearch == null)
                {
                    yield break;
                }

                // Ignore already searched properties
                if (context.History.Contains(objectToSearch))
                {
                    _loggers.TraceObject($"Already searched. Ignoring", objectToSearch);
                    yield break;
                }
                else
                {
                    context.History.Add(objectToSearch);
                }

                if (objectToSearch is T foundValue)
                {
                    if (_conditions.All(x => x(foundValue)))
                    {
                        _loggers.TraceObject($"Found ", foundValue);
                        yield return foundValue;
                    }
                    else
                    {
                        _loggers.TraceObject($"Found value but did not pass all conditions", objectToSearch);
                    }                                   
                }

                
                // Search properties
                foreach (var property in objectToSearch.GetType().GetProperties(_bindingFlags).Where(x => x.GetIndexParameters().Length == 0))
                {
                    var value = property.GetValue(objectToSearch);

                    if (value == null) continue;

                    if(!(_ignoredProperties.HasValue() && _ignoredProperties.Any(x => x(objectToSearch, property, value))))
                    {
                        foreach (var subValue in SearchObject(context, value))
                        {
                            yield return subValue;
                        }
                    }
                    else
                    {
                        _loggers.Debug($"Property <{property.Name}> on <{objectToSearch}> is ignored");
                    }

                    // Search elements if collection
                    if (value is IEnumerable propertyCollection && !_defaultIgnoredSystemTypes.Any(x => x(propertyCollection)))
                    {
                        _loggers.Debug($"<{objectToSearch}> which is a collection. Searching elements");
                        foreach (var item in propertyCollection)
                        {
                            foreach (var subValue in SearchObject(context, item))
                            {
                                yield return subValue;
                            }
                        }

                    }
                }

                // Search elements if collection
                if (objectToSearch is IEnumerable collection && !_defaultIgnoredSystemTypes.Any(x => x(collection)))
                {
                    _loggers.Debug($"<{objectToSearch}> which is a collection. Searching elements");
                    foreach (var item in collection)
                    {
                        foreach (var subValue in SearchObject(context, item))
                        {
                            yield return subValue;
                        }
                    }

                }
            }
        }

        private class SearchContext{
            public List<object> History { get; } = new List<object>();
        }
    }
}
