using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Reflection;
using Sels.ObjectValidationFramework.Profile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.ObjectValidationFramework
{
    /// <summary>
    /// Defines a profile that can be used to validate objects of type <typeparamref name="T"/> and returns errors of type <typeparamref name="TError"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated</typeparam>
    /// <typeparam name="TError">The type of the validation errors returned by the profile</typeparam>
    /// <typeparam name="TContext">The type of the context that can be used by the validation configuration</typeparam>
    public interface IValidationProfile<T, TError, TContext>
    {
        /// <summary>
        /// Validates <paramref name="objectToValidate"/> using the configuration in the profile and returns alls validation errors.
        /// </summary>
        /// <param name="objectToValidate">The object to validate</param>
        /// <param name="context">Optional context that can be used by the validation configuration</param>
        /// <param name="options"><inheritdoc cref="ProfileExecutionOptions"/></param>
        /// <returns>The validation result for <paramref name="objectToValidate"/></returns>
        ValidationResult<T, TError> Validate(T objectToValidate, TContext context = default, ProfileExecutionOptions options = ProfileExecutionOptions.None);
        /// <summary>
        /// Validates <paramref name="objectToValidate"/> using the configuration in the profile and returns alls validation errors.
        /// </summary>
        /// <param name="objectToValidate">The object to validate</param>
        /// <param name="context">Optional context that can be used by the validation configuration</param>
        /// <param name="options"><inheritdoc cref="ProfileExecutionOptions"/></param>
        /// <returns>The validation result for <paramref name="objectToValidate"/></returns>
        Task<ValidationResult<T, TError>> ValidateAsync(T objectToValidate, TContext context = default, ProfileExecutionOptions options = ProfileExecutionOptions.None);
    }
}
