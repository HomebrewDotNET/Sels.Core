using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Contracts.Rules;
using System;
using System.Collections.Generic;
using System.Text;

// Adjusted so extensions are available when using the ValidationProfile
namespace Sels.ObjectValidationFramework.Templates.Profile
{
    /// <summary>
    /// Contains generic configuration extensions for comparable types.
    /// </summary>
    public static class ComparableValidationExtensions
    {
        /// <summary>
        /// Value is only valid when it is larger than <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparator">Object to compare value to</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> MustBeLargerThan<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, IComparable comparator, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor) where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            comparator.ValidateArgument(nameof(comparator));

            return configurator.ValidIf(info => info.Value.CompareTo(comparator) > 0, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is larger than <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparator">Object to compare value to</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> MustBeLargerThan<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, IComparable comparator, bool includeParents = true) where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            comparator.ValidateArgument(nameof(comparator));

            return configurator.ValidIf(info => info.Value.CompareTo(comparator) > 0, info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be larger than <{comparator}>. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is larger or equal to <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparator">Object to compare value to</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> MustBeLargerOrEqualTo<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, IComparable comparator, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor) where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            comparator.ValidateArgument(nameof(comparator));

            return configurator.ValidIf(info => info.Value.CompareTo(comparator) >= 0, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is larger or equal to <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparator">Object to compare value to</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> MustBeLargerOrEqualTo<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, IComparable comparator, bool includeParents = true) where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            comparator.ValidateArgument(nameof(comparator));

            return configurator.ValidIf(info => info.Value.CompareTo(comparator) >= 0, info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be larger or equal to <{comparator}>. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is smaller than <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparator">Object to compare value to</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> MustBeSmallerThan<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, IComparable comparator, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor) where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            comparator.ValidateArgument(nameof(comparator));

            return configurator.ValidIf(info => info.Value.CompareTo(comparator) < 0, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is smaller than <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparator">Object to compare value to</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> MustBeSmallerThan<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, IComparable comparator, bool includeParents = true) where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            comparator.ValidateArgument(nameof(comparator));

            return configurator.ValidIf(info => info.Value.CompareTo(comparator) < 0, info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be smaller than <{comparator}>. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is smaller or equal to <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparator">Object to compare value to</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> MustBeSmallerOrEqualTo<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, IComparable comparator, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor) where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            comparator.ValidateArgument(nameof(comparator));

            return configurator.ValidIf(info => info.Value.CompareTo(comparator) <= 0, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is smaller or equal to <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparator">Object to compare value to</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> MustBeSmallerOrEqualTo<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, IComparable comparator, bool includeParents = true) where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            comparator.ValidateArgument(nameof(comparator));

            return configurator.ValidIf(info => info.Value.CompareTo(comparator) <= 0, info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be smaller or equal to <{comparator}>. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is between <paramref name="minComparator"/> and <paramref name="maxComparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minComparator">Object to compare value to</param>
        /// <param name="maxComparator">Object to compare value to</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> MustBeBetween<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, IComparable minComparator, IComparable maxComparator, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor) where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            minComparator.ValidateArgument(nameof(minComparator));
            maxComparator.ValidateArgument(nameof(maxComparator));

            return configurator.ValidIf(info => info.Value.CompareTo(minComparator) > 0 && info.Value.CompareTo(maxComparator) < 0, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is between <paramref name="minComparator"/> and <paramref name="maxComparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minComparator">Object to compare value to</param>
        /// <param name="maxComparator">Object to compare value to</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> MustBeBetween<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, IComparable minComparator, IComparable maxComparator, bool includeParents = true) where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            minComparator.ValidateArgument(nameof(minComparator));
            maxComparator.ValidateArgument(nameof(maxComparator));

            return configurator.ValidIf(info => info.Value.CompareTo(minComparator) > 0 && info.Value.CompareTo(maxComparator) < 0, info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be larger than <{minComparator}> and smaller than <{maxComparator}>. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is larger or equal to <paramref name="minComparator"/> and <paramref name="maxComparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minComparator">Object to compare value to</param>
        /// <param name="maxComparator">Object to compare value to</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> MustBeInRange<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, IComparable minComparator, IComparable maxComparator, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor) where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            minComparator.ValidateArgument(nameof(minComparator));
            maxComparator.ValidateArgument(nameof(maxComparator));

            return configurator.ValidIf(info => info.Value.CompareTo(minComparator) >= 0 && info.Value.CompareTo(maxComparator) <= 0, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is between <paramref name="minComparator"/> and <paramref name="maxComparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minComparator">Object to compare value to</param>
        /// <param name="maxComparator">Object to compare value to</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> MustBeInRange<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, IComparable minComparator, IComparable maxComparator, bool includeParents = true) where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            minComparator.ValidateArgument(nameof(minComparator));
            maxComparator.ValidateArgument(nameof(maxComparator));

            return configurator.ValidIf(info => info.Value.CompareTo(minComparator) >= 0 && info.Value.CompareTo(maxComparator) <= 0, info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be larger or equal to <{minComparator}> and smaller or equal to <{maxComparator}>. Was <{info.Value}>");
        }
    }
}
