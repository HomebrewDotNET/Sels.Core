using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Rules;
using System;

// Adjusted so extensions are available when using the ValidationProfile
namespace Sels.ObjectValidationFramework.Profile
{
    /// <summary>
    /// Contains generic configuration extensions for datetimes.
    /// </summary>
    public static class DateTimeValidationExtensions
    {
        /// <summary>
        /// Value is only valid when it is a date in the future.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, DateTime> MustBeInTheFuture<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, DateTime> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, DateTime>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value.IsInFuture(), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is a date in the future.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, DateTime> MustBeInTheFuture<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, DateTime> configurator, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.IsInFuture(), info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be in the future. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is today or a date in the future.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, DateTime> MustBeTodayOrInTheFuture<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, DateTime> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, DateTime>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value.IsToday() || info.Value.IsInFuture(), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is today or a date in the future.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, DateTime> MustBeTodayOrInTheFuture<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, DateTime> configurator, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.IsToday() || info.Value.IsInFuture(), info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be today or in the future. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is a date in the past.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, DateTime> MustBeInThePast<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, DateTime> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, DateTime>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value.IsInPast(), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is a date in the past.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, DateTime> MustBeInThePast<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, DateTime> configurator, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.IsInPast(), info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be in the past. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is today or a date in the past.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, DateTime> MustBeTodayOrInThePast<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, DateTime> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, DateTime>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value.IsToday() || info.Value.IsInPast(), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is today or a date in the past.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, DateTime> MustBeTodayOrInThePast<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, DateTime> configurator, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.IsToday() || info.Value.IsInPast(), info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be today or in the past. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is after <paramref name="date"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="date">Date to compare value against</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, DateTime> MustBeAfter<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, DateTime> configurator, DateTime date, Func<IValidationRuleContext<TEntity, TInfo, TContext, DateTime>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value > date, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is after <paramref name="date"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="date">Date to compare value against</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, DateTime> MustBeAfter<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, DateTime> configurator, DateTime date, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value > date, info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be before <{date}>. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is before <paramref name="date"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="date">Date to compare value against</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, DateTime> MustBeBefore<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, DateTime> configurator, DateTime date, Func<IValidationRuleContext<TEntity, TInfo, TContext, DateTime>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value < date, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is before <paramref name="date"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="date">Date to compare value against</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, DateTime> MustBeBefore<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, DateTime> configurator, DateTime date, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value < date, info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be before <{date}>. Was <{info.Value}>");
        }
    }
}
