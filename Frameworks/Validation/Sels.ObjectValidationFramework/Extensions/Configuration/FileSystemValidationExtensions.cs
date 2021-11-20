using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Contracts.Rules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// Adjusted so extensions are available when using the ValidationProfile
namespace Sels.ObjectValidationFramework.Templates.Profile
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
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> MustExists<TEntity, TError, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, TValue> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, TValue>, TError> errorConstructor) where TValue : FileSystemInfo
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
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <typeparam name="TValue">Type of value that is being validated</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> MustExists<TEntity, TInfo, TContext, TValue>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, TValue> configurator, bool includeParents = true) where TValue : FileSystemInfo
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => info.Value.Exists, info => $"{info.GetFullDisplayNameDynamically(includeParents)} must exist on the filesystem. Path was <{info.Value.FullName}>");
        }
    }
}
