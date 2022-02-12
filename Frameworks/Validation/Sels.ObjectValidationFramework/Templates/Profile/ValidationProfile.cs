using Microsoft.Extensions.Logging;
using Sels.Core.Components.ScopedActions;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Execution;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.Core.Extensions.Reflection;
using Sels.ObjectValidationFramework.Components.Validators;
using Sels.ObjectValidationFramework.Contracts.Validators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sels.ObjectValidationFramework.Templates.Profile
{
    /// <summary>
    /// Profile that can be configured using a fluent syntax to validate object of a certain type.
    /// </summary>
    /// <typeparam name="TError">Type of validation error to return</typeparam>
    public class ValidationProfile<TError>
    {
        // Static
        private static readonly Type[] _specialIgnoredCollectionTypes = new Type[]
        {
            typeof(string)
        };
        private static readonly Predicate<(object Property, PropertyInfo Info, object Context)>[] _defaultIgnoredPropertyConditions = new Predicate<(object Property, PropertyInfo Info, object Context)>[]
        {
            // Ignore all system types 
            new Predicate<(object Property, PropertyInfo Info, object Context)>(x => x.Info.PropertyType.FullName.StartsWith("System.")),
            // Ignore all microsoft types
            new Predicate<(object Property, PropertyInfo Info, object Context)>(x => x.Info.PropertyType.FullName.StartsWith("Microsoft."))
        };

        // Fields
        private readonly ILogger[] _loggers;
        internal readonly List<EntityValidator<TError>> _validators = new List<EntityValidator<TError>>();
        internal readonly List<Predicate<(object Property, PropertyInfo Info, object Context)>> _ignoredPropertyConditions = new List<Predicate<(object Property, PropertyInfo Info, object Context)>>();

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="loggers">Optional loggers for tracing</param>
        public ValidationProfile(IEnumerable<ILogger> loggers = null)
        {
            _loggers = loggers.ToArrayOrDefault();
        }

        #region Configuration
        /// <summary>
        /// Creates a new configurator for creating validation for objetcs of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to create validation for</typeparam>
        /// <returns>A configurator for creating validation</returns>
        public IValidationConfigurator<TEntity, TError> CreateValidationFor<TEntity>()
        {
            using (_loggers.TraceMethod(this))
            {
                var validator = new EntityValidator<TEntity, TError>(_loggers);
                _validators.Add(validator);
                return validator;
            }
        }
        #endregion

        #region Ignore For Fallthrough
        /// <summary>
        /// Validation will not be called on the property or elements from a collection if <paramref name="condition"/> returns true.
        /// </summary>
        /// <param name="condition">Predicate that dictates when when a property/collection is ignored for fallthrough</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> IgnoreForFallthrough(Predicate<(object Property, PropertyInfo Info, object Context)> condition)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                _ignoredPropertyConditions.Add(condition);
                return this;
            }
        }

        /// <summary>
        /// Validation will not be called on the property or elements from a collection if <paramref name="condition"/> returns true. Only gets executed when property can be assigned to <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of the property value</typeparam>
        /// <param name="condition">Predicate that dictates when when a property/collection is ignored for fallthrough</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> IgnoreForFallthrough<TEntity>(Predicate<(TEntity Property, PropertyInfo Info, object Context)> condition)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                return IgnoreForFallthrough(x =>
                {
                    if(x.Property is TEntity typedProperty)
                    {
                        return condition((typedProperty, x.Info, x.Context));
                    }

                    return false;
                });
            }
        }

        /// <summary>
        /// Validation will not be called on the property or elements from a collection if <paramref name="condition"/> returns true. Only gets executed when context can be assigned to <typeparamref name="TContext"/>.
        /// </summary>
        /// <typeparam name="TContext">Type of the supplied context</typeparam>
        /// <param name="condition">Predicate that dictates when when a property/collection is ignored for fallthrough</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> IgnoreForFallthroughWhenContext<TContext>(Predicate<(object Property, PropertyInfo Info, TContext Context)> condition)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                return IgnoreForFallthrough(x =>
                {
                    if (x.Context is TContext typedContext)
                    {
                        return condition((x.Property, x.Info, typedContext));
                    }

                    return false;
                });
            }
        }

        /// <summary>
        /// Validation will not be called on the property or elements from a collection if <paramref name="condition"/> returns true. Only gets executed when context can be assigned to <typeparamref name="TContext"/> and property can be assigned to <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of the property value</typeparam>
        /// <typeparam name="TContext">Type of the supplied context</typeparam>
        /// <param name="condition">Predicate that dictates when when a property/collection is ignored for fallthrough</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> IgnoreForFallthroughWhenContext<TContext, TEntity>(Predicate<(object Property, PropertyInfo Info, TContext Context)> condition)
        {
            using (_loggers.TraceMethod(this))
            {
                condition.ValidateArgument(nameof(condition));
                return IgnoreForFallthrough(x =>
                {
                    if (x.Property is TEntity typedProperty && x.Context is TContext typedContext)
                    {
                        return condition((typedProperty, x.Info, typedContext));
                    }

                    return false;
                });
            }
        }

        /// <summary>
        /// Validation will not be called for the property selected by <paramref name="property"/> on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of the entity <paramref name="property"/> is from</typeparam>
        /// <param name="property">Delegate that selects the property on <typeparamref name="TEntity"/></param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> IgnorePropertyForFallthrough<TEntity>(Expression<Func<TEntity, object>> property)
        {
            using (_loggers.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));

                return IgnorePropertyForFallthrough(property, x => true);
            }
        }

        /// <summary>
        /// Validation will not be called for the property selected by <paramref name="property"/> on <typeparamref name="TEntity"/>. Property will only be ignored when <paramref name="contextCondition"/> returns true.
        /// </summary>
        /// <typeparam name="TEntity">Type of the entity <paramref name="property"/> is from</typeparam>
        /// <param name="property">Delegate that selects the property on <typeparamref name="TEntity"/></param>
        /// <param name="contextCondition">Condition supplied context must pass before <paramref name="property"/> is ignored.</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> IgnorePropertyForFallthrough<TEntity>(Expression<Func<TEntity, object>> property, Predicate<object> contextCondition)
        {
            using (_loggers.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                contextCondition.ValidateArgument(nameof(contextCondition));

                return IgnorePropertyForFallthrough<TEntity, object>(property, contextCondition);
            }
        }

        /// <summary>
        /// Validation will not be called for the property selected by <paramref name="property"/> on <typeparamref name="TEntity"/>. Property will only be ignored when content is of type <typeparamref name="TContext"/> and when <paramref name="contextCondition"/> returns true.
        /// </summary>
        /// <typeparam name="TEntity">Type of the entity <paramref name="property"/> is from</typeparam>
        /// <typeparam name="TContext">Type of the context</typeparam>
        /// <param name="property">Delegate that selects the property on <typeparamref name="TEntity"/></param>
        /// <param name="contextCondition">Condition supplied context must pass before <paramref name="property"/> is ignored.</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> IgnorePropertyForFallthrough<TEntity, TContext>(Expression<Func<TEntity, object>> property, Predicate<TContext> contextCondition)
        {
            using (_loggers.TraceMethod(this))
            {
                property.ValidateArgument(nameof(property));
                if (!property.TryExtractProperty(out var propertyInfo) || !typeof(TEntity).IsAssignableTo(propertyInfo.ReflectedType))
                {
                    throw new ArgumentException($"{nameof(property)} must select a property on {typeof(TEntity)}");
                }
                contextCondition.ValidateArgument(nameof(contextCondition));

                return IgnoreForFallthrough(x =>
                {
                    if(x.Context is TContext typedContext)
                    {
                        return contextCondition(typedContext) && x.Info.Name == propertyInfo.Name && x.Info.DeclaringType == propertyInfo.DeclaringType;
                    }                   

                    return false;
                });
            }
        }
        #endregion

        #region Importing
        /// <summary>
        /// Import configuration from <paramref name="profile"/> into this profile.
        /// </summary>
        /// <param name="profile">Profile to import from</param>
        /// <param name="options">What to import</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> ImportFrom(ValidationProfile<TError> profile, ImportOptions options = ImportOptions.All)
        {
            using (_loggers.TraceMethod(this))
            {
                profile.ValidateArgument(nameof(profile));

                if(options.HasFlag(ImportOptions.Configuration))
                {
                    _loggers.Debug($"Importing configuration from <{profile}>");
                    _validators.AddRange(profile._validators);
                }

                if (options.HasFlag(ImportOptions.IgnoreConditions))
                {
                    _loggers.Debug($"Importing ignore conditions from <{profile}>");
                    _ignoredPropertyConditions.AddRange(profile._ignoredPropertyConditions);
                }

                return this;
            }
        }

        /// <summary>
        /// Import configuration from <typeparamref name="TProfile"/> into this profile.
        /// </summary>
        /// <typeparam name="TProfile">Type of profile to import options from</typeparam>
        /// <param name="options">What to import</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> ImportFrom<TProfile>(ImportOptions options = ImportOptions.All) where TProfile : ValidationProfile<TError>, new()
        {
            using (_loggers.TraceMethod(this))
            {
                return ImportFrom(new TProfile(), options);
            }
        }

        /// <summary>
        /// Import configuration from the profile created by <paramref name="constructor"/> into this profile.
        /// </summary>
        /// <param name="constructor">Delegate that creates the profile instance to import from</param>
        /// <param name="options">What to import</param>
        /// <returns>Current profile for method chaining</returns>
        public ValidationProfile<TError> ImportFrom(Func<ValidationProfile<TError>> constructor, ImportOptions options = ImportOptions.All)
        {
            using (_loggers.TraceMethod(this))
            {
                constructor.ValidateArgument(nameof(constructor));

                return ImportFrom(constructor(), options);
            }
        }
        #endregion

        #region Validation
        /// <summary>
        /// Validates <paramref name="objectToValidate"/> using the configuration in the profile and returns alls validation errors.
        /// </summary>
        /// <param name="objectToValidate">The object to validate</param>
        /// <param name="context">Optional context that can be used by the validation configuration</param>
        /// <returns>All validation errors for <paramref name="objectToValidate"/></returns>
        public TError[] Validate(object objectToValidate, object context = null)
        {
            using (_loggers.TraceMethod(this))
            {
                var profileContext = new ProfileValidationContext(context);

                using(_loggers.TraceAction($"Validating root object <{objectToValidate}>", x => $"Validated root object <{objectToValidate}> by checking {profileContext.History.Count} objects which returned {profileContext.Errors.Count} errors in {x.PrintTotalMs()}"))
                {
                    Validate(profileContext, objectToValidate, null);

                    var objectType = objectToValidate.GetType();
                    if (!_specialIgnoredCollectionTypes.Any(x => objectType.IsAssignableTo(x)) && objectType.IsContainer())
                    {
                        _loggers.Debug($"Root object <{objectToValidate}> is a collection. Triggering validation for the elements");
                        var counter = 0;
                        foreach (var element in objectToValidate.Cast<IEnumerable>())
                        {
                            Validate(profileContext, element, counter);
                        }
                        counter++;
                    }
                }

                return profileContext.Errors.ToArray();
            }
        }

        private void Validate(ProfileValidationContext profileContext, object objectToValidate, int? elementIndex = null)
        {
            using (_loggers.TraceMethod(this))
            {
                profileContext.ValidateArgument(nameof(profileContext));
                objectToValidate.ValidateArgument(nameof(objectToValidate));

                // Check if we arleady validated in an attemt to avoid duplicate validation and circular fallthrough
                if (profileContext.History.Contains(objectToValidate))
                {
                    _loggers.Debug($"Already validated <{objectToValidate}>. Skipping");
                    return;
                }
                else
                {
                    _loggers.Debug($"Starting validation for <{objectToValidate}>");
                    profileContext.History.Add(objectToValidate);
                }

                var currentParents = profileContext.CurrentParents.ToArray();

                // Trigger validation for the current object if we have validators for it
                var validators = _validators.Where(x => x.CanValidate(objectToValidate, profileContext.Context, elementIndex, currentParents)).ToArray();

                for(int i = 0; i < validators.Length; i++)
                {
                    var validator = validators[i];
                    _loggers.Debug($"Using validator {i+1} for <{objectToValidate}>");
                    var errors = validator.Validate(objectToValidate, profileContext.Context, elementIndex, currentParents);
                    _loggers.Debug($"Validator {i + 1} for <{objectToValidate}> returned {errors.Length} errors");
                    profileContext.Errors.AddRange(errors);
                }

                // Execute fallthrough
                foreach(var property in objectToValidate.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetIndexParameters().Length == 0))
                {
                    var value = property.GetValue(objectToValidate);

                    if(value != null)
                    {
                        // Check if ignored by profile
                        if (_ignoredPropertyConditions.Any(x => x((value, property, profileContext.Context))))
                        {
                            _loggers.Debug($"Property {property.Name} on <{objectToValidate}> is ignored. Skipping for fallthrough");
                            continue;
                        }

                        // Allowed if property is a collection but not any of the special collection types
                        var isAllowedForCollectionFallthough = !_specialIgnoredCollectionTypes.Any(x => property.PropertyType.IsAssignableTo(x)) && property.PropertyType.IsContainer();
                        // Allowed if the property doesn't have a validator explicitly defined and isn't a default ignored type
                        var isAllowedForPropertyFallthough = !_validators.Any(x => x.CanValidate(value, profileContext.Context, null, currentParents)) && !_defaultIgnoredPropertyConditions.Any(x => x((value, property, profileContext.Context)));

                        if(isAllowedForCollectionFallthough || isAllowedForPropertyFallthough)
                        {
                            var parent = new Parent(objectToValidate, property, elementIndex);
                            using (new ScopedAction(() => profileContext.CurrentParents.Add(parent), () => profileContext.CurrentParents.Remove(parent)))
                            {                              
                                if (isAllowedForCollectionFallthough)
                                {
                                    // Loop over elements in collection and trigger validation for the elements
                                    _loggers.Debug($"Property {property.Name} on <{objectToValidate}> is a collection. Triggering validation for the elements");
                                    var counter = 0;
                                    foreach (var element in value.Cast<IEnumerable>())
                                    {
                                        Validate(profileContext, element, counter);
                                    }
                                    counter++;
                                }

                                // Trigger validation for the property value
                                if (isAllowedForPropertyFallthough)
                                {
                                    _loggers.Debug($"Triggering fallthough for property {property.Name} on <{objectToValidate}>.");
                                    Validate(profileContext, value, 0);
                                }
                                else
                                {
                                    _loggers.Debug($"Property {property.Name} on <{objectToValidate}> is ignored by default. Skipping for property fallthrough");
                                }
                            }
                        }                                      
                    }
                    else
                    {
                        _loggers.Debug($"Property {property.Name} on <{objectToValidate}> was null. Skipping for fallthrough");
                    }                    
                }
            }
        }
        #endregion

        private class ProfileValidationContext
        {
            public ProfileValidationContext(object context)
            {
                Context = context;
            }

            public object Context { get; }
            public List<object> History { get; } = new List<object>();
            public List<TError> Errors { get; } = new List<TError>();
            public List<Parent> CurrentParents { get; } = new List<Parent>();

        }
    }

    /// <summary>
    /// Tells what to import from another profile.
    /// </summary>
    [Flags]
    public enum ImportOptions
    {
        /// <summary>
        /// Import everything
        /// </summary>
        All = IgnoreConditions + Configuration,
        /// <summary>
        /// Impors all conditions for what properties/collection to ignore for fallthough.
        /// </summary>
        IgnoreConditions = 1,
        /// <summary>
        /// Import all configured validation.
        /// </summary>
        Configuration = 2
    }
}
