using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.ObjectValidationFramework.Components.Rules;
using Sels.ObjectValidationFramework.Contracts.Rules;
using System;
using System.Collections.Generic;
using System.Text;

// Adjusted so extensions are available when using the ValidationProfile
namespace Sels.ObjectValidationFramework.Templates.Profile
{
    /// <summary>
    /// Extra extension methods for working the with validation rule context
    /// </summary>
    public static class ContextExtensions
    {
        /// <summary>
        /// Returns a display name for the current value that being validated.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="ruleContext">Validation rule context for the currently being validated value</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Display name representing the current value that's being validated</returns>
        public static string GetDisplayName<TEntity, TInfo, TContext, TValue>(this IValidationRuleContext<TEntity, TInfo, TContext, TValue> ruleContext, bool includeParents = true)
        {
            ruleContext.ValidateArgument(nameof(ruleContext));

            var builder = new StringBuilder();

            if (includeParents && ruleContext.Parents.HasValue())
            {
                for(int i = 0; i < ruleContext.Parents.Length; i++)
                {
                    var parent = ruleContext.Parents[i];

                    // First parent so we use type name as root
                    if(i == 0)
                    {
                        builder.Append(parent.Instance.GetType().Name);
                    }

                    // Append array like display when parent was part of collection
                    if (parent.ElementIndex.HasValue())
                    {
                        builder.Append($"[{parent.ElementIndex.Value}]");
                    }

                    // Append child property
                    builder.Append("." + parent.ChildProperty.Name);
                }
            }
            // No parents so we use entity name as root
            else
            {
                builder.Append(typeof(TEntity).Name);
            }

            // Append array like display when current value was part of collection
            if (ruleContext.ElementIndex.HasValue)
            {
                builder.Append($"[{ruleContext.ElementIndex.Value}]");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Returns a display name for the current value that being validated.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="ruleContext">Validation rule context for the currently being validated value</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Display name representing the current value that being's validated</returns>
        public static string GetFullDisplayName<TEntity, TContext, TValue>(this IValidationRuleContext<TEntity, NullValidationInfo, TContext, TValue> ruleContext, bool includeParents = true)
        {
            ruleContext.ValidateArgument(nameof(ruleContext));

            return ruleContext.GetDisplayName(includeParents);
        }

        /// <summary>
        /// Returns a display name for the current value that being validated. Includes the property name.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="ruleContext">Validation rule context for the currently being validated value</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Display name representing the current value that's being validated</returns>
        public static string GetFullDisplayName<TEntity, TContext, TValue>(this IValidationRuleContext<TEntity, PropertyValidationInfo, TContext, TValue> ruleContext, bool includeParents = true)
        {
            ruleContext.ValidateArgument(nameof(ruleContext));

            var builder = new StringBuilder();

            builder.Append(ruleContext.GetDisplayName(includeParents) + ".");

            // Include property name
            builder.Append(ruleContext.Info.Property.Name);

            return builder.ToString();
        }

        /// <summary>
        /// Returns a display name for the current value that being validated. Includes the property name with collection index.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="ruleContext">Validation rule context for the currently being validated value</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Display name representing the current value that's being validated</returns>
        public static string GetFullDisplayName<TEntity, TContext, TValue>(this IValidationRuleContext<TEntity, CollectionPropertyValidationInfo, TContext, TValue> ruleContext, bool includeParents = true)
        {
            ruleContext.ValidateArgument(nameof(ruleContext));

            var builder = new StringBuilder();

            builder.Append(ruleContext.GetDisplayName(includeParents) + ".");

            // Include property name with index
            builder.Append($"{ruleContext.Info.Property.Name}[{ruleContext.Info.ValueIndex}]");

            return builder.ToString();
        }

        /// <summary>
        /// Returns a display name for the current value that being validated. Dynamically selects the right overload.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="ruleContext">Validation rule context for the currently being validated value</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Display name representing the current value that's being validated</returns>
        public static string GetFullDisplayNameDynamically<TEntity, TInfo, TContext, TValue>(this IValidationRuleContext<TEntity, TInfo, TContext, TValue> ruleContext, bool includeParents = true)
        {
            ruleContext.ValidateArgument(nameof(ruleContext));

            return ruleContext.Cast<dynamic>().GetFullDisplayName(includeParents);
        }
    }
}
