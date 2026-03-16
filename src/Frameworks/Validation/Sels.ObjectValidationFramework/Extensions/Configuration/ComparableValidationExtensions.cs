using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Configurators;
using Sels.ObjectValidationFramework.Rules;
using System;

// Adjusted so extensions are available when using the ValidationProfile
namespace Sels.ObjectValidationFramework.Profile
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparator">Object to compare value to</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustBeLargerThan<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, IComparable comparator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            comparator.ValidateArgument(nameof(comparator));
            comparator.ValidateArgumentAssignableTo(nameof(comparator), typeof(TValue));

            return configurator.ValidIf(info => info.Value.CompareTo(comparator) > 0, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is larger than <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparator">Object to compare value to</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustBeLargerThan<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, IComparable comparator)
            where TTargetContext : TBaseContext
            where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            comparator.ValidateArgument(nameof(comparator));
            comparator.ValidateArgumentAssignableTo(nameof(comparator), typeof(TValue));

            return configurator.ValidIf(info => info.Value.CompareTo(comparator) > 0, info => $"Must be larger than <{comparator}>. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is larger or equal to <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparator">Object to compare value to</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustBeLargerOrEqualTo<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, IComparable comparator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            comparator.ValidateArgument(nameof(comparator));
            comparator.ValidateArgumentAssignableTo(nameof(comparator), typeof(TValue));

            return configurator.ValidIf(info => info.Value.CompareTo(comparator) >= 0, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is larger or equal to <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparator">Object to compare value to</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustBeLargerOrEqualTo<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, IComparable comparator)
            where TTargetContext : TBaseContext
            where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            comparator.ValidateArgument(nameof(comparator));
            comparator.ValidateArgumentAssignableTo(nameof(comparator), typeof(TValue));

            return configurator.ValidIf(info => info.Value.CompareTo(comparator) >= 0, info => $"Must be larger or equal to <{comparator}>. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is smaller than <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparator">Object to compare value to</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustBeSmallerThan<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, IComparable comparator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            comparator.ValidateArgument(nameof(comparator));
            comparator.ValidateArgumentAssignableTo(nameof(comparator), typeof(TValue));

            return configurator.ValidIf(info => info.Value.CompareTo(comparator) < 0, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is smaller than <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparator">Object to compare value to</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustBeSmallerThan<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, IComparable comparator)
            where TTargetContext : TBaseContext
            where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            comparator.ValidateArgument(nameof(comparator));
            comparator.ValidateArgumentAssignableTo(nameof(comparator), typeof(TValue));

            return configurator.ValidIf(info => info.Value.CompareTo(comparator) < 0, info => $"Must be smaller than <{comparator}>. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is smaller or equal to <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparator">Object to compare value to</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustBeSmallerOrEqualTo<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, IComparable comparator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            comparator.ValidateArgument(nameof(comparator));
            comparator.ValidateArgumentAssignableTo(nameof(comparator), typeof(TValue));

            return configurator.ValidIf(info => info.Value.CompareTo(comparator) <= 0, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is smaller or equal to <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="comparator">Object to compare value to</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustBeSmallerOrEqualTo<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, IComparable comparator)
            where TTargetContext : TBaseContext
            where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            comparator.ValidateArgument(nameof(comparator));
            comparator.ValidateArgumentAssignableTo(nameof(comparator), typeof(TValue));

            return configurator.ValidIf(info => info.Value.CompareTo(comparator) <= 0, info => $"Must be smaller or equal to <{comparator}>. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is between <paramref name="minComparator"/> and <paramref name="maxComparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minComparator">Object to compare value to</param>
        /// <param name="maxComparator">Object to compare value to</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustBeBetween<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, IComparable minComparator, IComparable maxComparator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            minComparator.ValidateArgument(nameof(minComparator));
            maxComparator.ValidateArgument(nameof(maxComparator));
            minComparator.ValidateArgumentAssignableTo(nameof(minComparator), typeof(TValue));
            maxComparator.ValidateArgumentAssignableTo(nameof(maxComparator), typeof(TValue));

            return configurator.ValidIf(info => info.Value.CompareTo(minComparator) > 0 && info.Value.CompareTo(maxComparator) < 0, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is between <paramref name="minComparator"/> and <paramref name="maxComparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minComparator">Object to compare value to</param>
        /// <param name="maxComparator">Object to compare value to</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustBeBetween<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, IComparable minComparator, IComparable maxComparator)
            where TTargetContext : TBaseContext
            where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            minComparator.ValidateArgument(nameof(minComparator));
            maxComparator.ValidateArgument(nameof(maxComparator));
            minComparator.ValidateArgumentAssignableTo(nameof(minComparator), typeof(TValue));
            maxComparator.ValidateArgumentAssignableTo(nameof(maxComparator), typeof(TValue));

            return configurator.ValidIf(info => info.Value.CompareTo(minComparator) > 0 && info.Value.CompareTo(maxComparator) < 0, info => $"Must be larger than <{minComparator}> and smaller than <{maxComparator}>. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is larger or equal to <paramref name="minComparator"/> and <paramref name="maxComparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minComparator">Object to compare value to</param>
        /// <param name="maxComparator">Object to compare value to</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustBeInRange<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, IComparable minComparator, IComparable maxComparator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            minComparator.ValidateArgument(nameof(minComparator));
            maxComparator.ValidateArgument(nameof(maxComparator));
            minComparator.ValidateArgumentAssignableTo(nameof(minComparator), typeof(TValue));
            maxComparator.ValidateArgumentAssignableTo(nameof(maxComparator), typeof(TValue));

            return configurator.ValidIf(info => info.Value.CompareTo(minComparator) >= 0 && info.Value.CompareTo(maxComparator) <= 0, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is between <paramref name="minComparator"/> and <paramref name="maxComparator"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minComparator">Object to compare value to</param>
        /// <param name="maxComparator">Object to compare value to</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustBeInRange<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, IComparable minComparator, IComparable maxComparator)
            where TTargetContext : TBaseContext
            where TValue : IComparable
        {
            configurator.ValidateArgument(nameof(configurator));
            minComparator.ValidateArgument(nameof(minComparator));
            maxComparator.ValidateArgument(nameof(maxComparator));
            minComparator.ValidateArgumentAssignableTo(nameof(minComparator), typeof(TValue));
            maxComparator.ValidateArgumentAssignableTo(nameof(maxComparator), typeof(TValue));

            return configurator.ValidIf(info => info.Value.CompareTo(minComparator) >= 0 && info.Value.CompareTo(maxComparator) <= 0, info => $"Must be larger or equal to <{minComparator}> and smaller or equal to <{maxComparator}>. Was <{info.Value}>");
        }
    }
}
