using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Configurators;
using Sels.ObjectValidationFramework.Rules;
using System;
using System.IO;

// Adjusted so extensions are available when using the ValidationProfile
namespace Sels.ObjectValidationFramework.Profile
{
    /// <summary>
    /// Contains generic configuration extensions for file system types.
    /// </summary>
    public static class FileSystemValidationExtensions
    {
        /// <summary>
        /// Value is only valid when it exists on the file system.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> MustExist<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, TValue> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, TValue>, TError> errorConstructor)
            where TTargetContext : TBaseContext
            where TValue : FileSystemInfo
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => info.Value.Exists, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it exists on the file system.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> MustExist<TEntity, TBaseContext, TInfo, TTargetContext, TValue>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, TValue> configurator)
            where TTargetContext : TBaseContext
            where TValue : FileSystemInfo
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.Exists, info => $"Must exist on the filesystem. Path was <{info.Value.FullName}>");
        }
    }
}
