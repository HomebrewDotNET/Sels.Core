using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Configurators;
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, DateTime> MustBeInTheFuture<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, DateTime> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, DateTime>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, DateTime> MustBeInTheFuture<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, DateTime> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.IsInFuture(), info => $"Must be in the future. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is today or a date in the future.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, DateTime> MustBeTodayOrInTheFuture<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, DateTime> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, DateTime>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, DateTime> MustBeTodayOrInTheFuture<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, DateTime> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.IsToday() || info.Value.IsInFuture(), info => $"Must be today or in the future. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is a date in the past.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, DateTime> MustBeInThePast<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, DateTime> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, DateTime>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, DateTime> MustBeInThePast<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, DateTime> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.IsInPast(), info => $"Must be in the past. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is today or a date in the past.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, DateTime> MustBeTodayOrInThePast<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, DateTime> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, DateTime>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, DateTime> MustBeTodayOrInThePast<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, DateTime> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.IsToday() || info.Value.IsInPast(), info => $"Must be today or in the past. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is after <paramref name="date"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="date">Date to compare value against</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, DateTime> MustBeAfter<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, DateTime> configurator, DateTime date, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, DateTime>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="date">Date to compare value against</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, DateTime> MustBeAfter<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, DateTime> configurator, DateTime date)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value > date, info => $"Must be before <{date}>. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is before <paramref name="date"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="date">Date to compare value against</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, DateTime> MustBeBefore<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, DateTime> configurator, DateTime date, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, DateTime>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="date">Date to compare value against</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, DateTime> MustBeBefore<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, DateTime> configurator, DateTime date)
            where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value < date, info => $"Must be before <{date}>. Was <{info.Value}>");
        }
    }
}
