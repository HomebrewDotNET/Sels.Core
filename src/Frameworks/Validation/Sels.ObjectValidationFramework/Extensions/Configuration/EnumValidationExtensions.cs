using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Configurators;
using Sels.ObjectValidationFramework.Rules;
using System;
using System.Collections.Generic;
using System.Text;

// Adjusted so extensions are available when using the ValidationProfile
namespace Sels.ObjectValidationFramework.Profile
{
    /// <summary>
    /// Contains generic configuration extensions for enumeration validation.
    /// </summary>
    public static class EnumValidationExtensions
    {
        /// <summary>
        /// Value is only valid when it is a defined value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Function to create validation error</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> IsDefined<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : Enum
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => Enum.IsDefined(typeof(TValue), info.Value), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is a defined value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> IsDefined<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator)
            where TTargetContext : TBaseContext
            where TValue : Enum
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.IsDefined(x => $"Must be a defined value of {typeof(TValue).Name}");
        }

        /// <summary>
        /// Value is only valid when it has the specified <paramref name="flag"/> set.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of error that is returned when the validation rule fails</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="flag">Flag that the value must have set</param>
        /// <param name="errorConstructor">Function to create the error when the validation rule fails</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> HasFlag<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, TValue flag, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : Enum
        {
                        configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value.HasFlag(flag), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it has the specified <paramref name="flag"/> set.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="flag">Flag that the value must have set</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> HasFlag<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator, TValue flag)
            where TTargetContext : TBaseContext
            where TValue : Enum
        {
            configurator.ValidateArgument(nameof(configurator));
            return configurator.HasFlag(flag, x => $"Must have the flag {flag} set");
        }
    }
}
