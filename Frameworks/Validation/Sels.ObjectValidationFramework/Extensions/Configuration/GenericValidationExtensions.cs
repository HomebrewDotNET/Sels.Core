using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Contracts.Rules;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions.Reflection;
using System.Linq;

// Adjusted so extensions are available when using the ValidationProfile
namespace Sels.ObjectValidationFramework.Templates.Profile
{
    /// <summary>
    /// Contains generic configuration extensions. 
    /// </summary>
    public static class GenericValidationExtensions
    {
        /// <summary>
        /// Value is only valid when it is not null.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> CannotBeNull<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.InvalidIf(info => info.Value == null, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is not null.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> CannotBeNull<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.InvalidIf(info => info.Value == null, info => $"{info.GetFullDisplayNameDynamically(includeParents)} cannot be null");
        }

        /// <summary>
        /// Value is only valid when it is not the default value of the type.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> CannotBeDefault<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.InvalidIf(info => info.Value.IsDefault(), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is not the default value of the type.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> CannotBeDefault<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.InvalidIf(info => info.Value.IsDefault(), info => $"{info.GetFullDisplayNameDynamically(includeParents)} cannot be the default value. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is null.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> MustBeNull<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value == null, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is null.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> MustBeNull<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value == null, info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be null");
        }

        /// <summary>
        /// Value is only valid when it is the default value of the type.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> MustBeDefault<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value.IsDefault(), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is not the default value of the type.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> MustBeDefault<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.IsDefault(), info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be the default value. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it is in <paramref name="validValues"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="validValues">Array of valid values</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> MustBeIn<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, TValue[] validValues, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            validValues.ValidateArgument(nameof(validValues));

            return configurator.ValidIf(info => validValues.Contains(info.Value), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is in <paramref name="validValues"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="validValues">Array of valid values</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> MustBeIn<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, TValue[] validValues, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));
            validValues.ValidateArgument(nameof(validValues));

            var validValuesString = validValues.JoinString(", ");
            return configurator.ValidIf(info => validValues.Contains(info.Value), info => $"{info.GetFullDisplayNameDynamically(includeParents)} is not in the list of valid values. Was <{info.Value}>. Must be in <{validValuesString}>");
        }

        #region Conditions
        /// <summary>
        /// Validation rules created after this method call will only be executed when value is not null.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> WhenNotNull<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidateWhen(info => info.Value != null);
        }

        /// <summary>
        /// Validation rules created after this method call will only be executed when value is not the default value for it's type.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> WhenNotDefault<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidateWhen(info => !info.Value.IsDefault());
        }
        #endregion
    }
}
