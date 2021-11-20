using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.ObjectValidationFramework.Contracts.Rules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

// Adjusted so extensions are available when using the ValidationProfile
namespace Sels.ObjectValidationFramework.Templates.Profile
{
    /// <summary>
    /// Contains generic configuration extensions for collection types.
    /// </summary>
    public static class CollectionValidationExtensions
    {
        /// <summary>
        /// Value is only valid when it contains at least 1 element.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> CannotBeEmpty<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor) where TValue : IEnumerable<object>
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value.GetCount() > 0, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it contains at least 1 element.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> CannotBeEmpty<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, bool includeParents = true) where TValue : IEnumerable<object>
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.GetCount() > 0, info => $"{info.GetFullDisplayNameDynamically(includeParents)} must contain at least 1 element. Contained <{info.Value.GetCount()}>");
        }

        /// <summary>
        /// Value is only valid when it doesn't contain any elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> MustBeEmpty<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor) where TValue : IEnumerable<object>
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value.GetCount() == 0, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it doesn't contain any elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> MustBeEmpty<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, bool includeParents = true) where TValue : IEnumerable<object>
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.GetCount() == 0, info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be empty. Contained <{info.Value.GetCount()}>");
        }

        /// <summary>
        /// Value is only valid when it contains <paramref name="count"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="count">How many elements the collection must contain</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> MustContain<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, int count, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor) where TValue : IEnumerable<object>
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            count.ValidateArgumentLarger(nameof(count), 0);

            return configurator.ValidIf(info => info.Value.GetCount() == count, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it contains <paramref name="count"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="count">How many elements the collection must contain</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> MustContain<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, int count, bool includeParents = true) where TValue : IEnumerable<object>
        {
            configurator.ValidateArgument(nameof(configurator));
            count.ValidateArgumentLarger(nameof(count), 0);

            return configurator.ValidIf(info => info.Value.GetCount() == count, info => $"{info.GetFullDisplayNameDynamically(includeParents)} must contain {count} elements. Contained <{info.Value.GetCount()}>");
        }

        /// <summary>
        /// Value is only valid when it only contains a max of <paramref name="count"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="count">Max allowed count of elements</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> MustContainAtMax<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, int count, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor) where TValue : IEnumerable<object>
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            count.ValidateArgumentLargerOrEqual(nameof(count), 0);

            return configurator.ValidIf(info => info.Value.GetCount() <= count, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it only contains a max of <paramref name="count"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="count">Max allowed count of elements</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> MustContainAtMax<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, int count, bool includeParents = true) where TValue : IEnumerable<object>
        {
            configurator.ValidateArgument(nameof(configurator));
            count.ValidateArgumentLargerOrEqual(nameof(count), 0);

            return configurator.ValidIf(info => info.Value.GetCount() <= count, info => $"{info.GetFullDisplayNameDynamically(includeParents)} can only contain a maximum of {count} elements. Contained <{info.Value.GetCount()}>");
        }

        /// <summary>
        /// Value is only valid when it only contains a min of <paramref name="count"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="count">Max allowed count of elements</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> MustContainAtLeast<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, int count, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor) where TValue : IEnumerable<object>
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            count.ValidateArgumentLargerOrEqual(nameof(count), 0);

            return configurator.ValidIf(info => info.Value.GetCount() >= count, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it only contains a min of <paramref name="count"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="count">Max allowed count of elements</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> MustContainAtLeast<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, int count, bool includeParents = true) where TValue : IEnumerable<object>
        {
            configurator.ValidateArgument(nameof(configurator));
            count.ValidateArgumentLargerOrEqual(nameof(count), 0);

            return configurator.ValidIf(info => info.Value.GetCount() >= count, info => $"{info.GetFullDisplayNameDynamically(includeParents)} can only contain a minimum of {count} elements. Contained <{info.Value.GetCount()}>");
        }

        /// <summary>
        /// Value is only valid when it contains more or equal to <paramref name="minCount"/> and less or equal to <paramref name="maxCount"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minCount">Min allowed count of elements</param>
        /// <param name="maxCount">Max allowed count of elements</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> MustContainInRange<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, int minCount, int maxCount, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor) where TValue : IEnumerable<object>
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            minCount.ValidateArgumentLargerOrEqual(nameof(minCount), 0);
            maxCount.ValidateArgumentLarger(nameof(maxCount), minCount);

            return configurator.ValidIf(info => info.Value.GetCount() >= minCount && info.Value.GetCount() <= maxCount, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it contains more or equal to <paramref name="minCount"/> and less or equal to <paramref name="maxCount"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minCount">Min allowed count of elements</param>
        /// <param name="maxCount">Max allowed count of elements</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> MustContainInRange<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, int minCount, int maxCount, bool includeParents = true) where TValue : IEnumerable<object>
        {
            configurator.ValidateArgument(nameof(configurator));
            minCount.ValidateArgumentLargerOrEqual(nameof(minCount), 0);
            maxCount.ValidateArgumentLarger(nameof(maxCount), minCount);

            return configurator.ValidIf(info => info.Value.GetCount() >= minCount && info.Value.GetCount() <= maxCount, info => $"{info.GetFullDisplayNameDynamically(includeParents)} can only contain a minimum of {minCount} and a maximum of {maxCount} elements. Contained <{info.Value.GetCount()}>");
        }

        /// <summary>
        /// Value is only valid when it contains between <paramref name="minCount"/> and <paramref name="maxCount"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minCount">Min allowed count of elements</param>
        /// <param name="maxCount">Max allowed count of elements</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> MustContainInBetween<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, int minCount, int maxCount, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor) where TValue : IEnumerable<object>
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            minCount.ValidateArgumentLargerOrEqual(nameof(minCount), 0);
            maxCount.ValidateArgumentLarger(nameof(maxCount), minCount+1);

            return configurator.ValidIf(info => info.Value.GetCount() > minCount && info.Value.GetCount() < maxCount, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it contains more or equal to <paramref name="minCount"/> and less or equal to <paramref name="maxCount"/> elements.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minCount">Min allowed count of elements</param>
        /// <param name="maxCount">Max allowed count of elements</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> MustContainInBetween<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, int minCount, int maxCount, bool includeParents = true) where TValue : IEnumerable<object>
        {
            configurator.ValidateArgument(nameof(configurator));
            minCount.ValidateArgumentLargerOrEqual(nameof(minCount), 0);
            maxCount.ValidateArgumentLarger(nameof(maxCount), minCount+1);

            return configurator.ValidIf(info => info.Value.GetCount() > minCount && info.Value.GetCount() < maxCount, info => $"{info.GetFullDisplayNameDynamically(includeParents)} can only contain between {minCount} and {maxCount} elements. Contained <{info.Value.GetCount()}>");
        }
    }
}
