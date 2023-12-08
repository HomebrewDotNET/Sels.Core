using Sels.Core.Extensions;
using Sels.Core.Extensions.Validation;
using Sels.Core.Validation;
using Sels.ObjectValidationFramework.Profile;
using System;
using System.Linq;

namespace Sels.ObjectValidationFramework.Extensions.Validation
{
    /// <summary>
    /// Provides extra extension for validating objects with profiles.
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Validates <paramref name="objectToValidate"/> using a profile of type <typeparamref name="TProfile"/>.
        /// </summary>
        /// <typeparam name="TProfile">Type of profile to use for validation</typeparam>
        /// <typeparam name="TEntity">The type of the entity to validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <param name="objectToValidate">Object to validate</param>
        /// <param name="context">Optional object containing additional information for the validators</param>
        /// <returns>The validation result for <paramref name="objectToValidate"/></returns>
        public static ValidationResult<TEntity, TError> ValidateAgainstProfile<TProfile, TEntity, TError>(this TEntity objectToValidate, object context = null) where TProfile : ValidationProfile<TError>, new()
        {
            objectToValidate.ValidateArgument(nameof(objectToValidate));

            return new TProfile().Validate(objectToValidate, context);
        }

        /// <summary>
        /// Will throw a <see cref="ValidationException{TEntity, TError}"/> when <paramref name="validationResult"/> contains errors.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity that was validated</typeparam>
        /// <typeparam name="TError">The type of validation errors returned</typeparam>
        /// <param name="validationResult">The validation result to check</param>
        /// <param name="useFullDisplayName">True to include the full parent hierarchy of validated objects, false to not include the parents</param>
        public static void ThrowOnValidationErrors<TEntity, TError>(this ValidationResult<TEntity, TError> validationResult, bool useFullDisplayName = true)
        {
            validationResult.ValidateArgument(nameof(validationResult));

            validationResult.ThrowOnValidationErrors(x => $"{(useFullDisplayName ? x.FullDisplayName : x.DisplayName)}: {x.Message}");
        }
        /// <summary>
        /// Will throw a <see cref="ValidationException{TEntity, TError}"/> when <paramref name="validationResult"/> contains errors.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity that was validated</typeparam>
        /// <typeparam name="TError">The type of validation errors returned</typeparam>
        /// <param name="validationResult">The validation result to check</param>
        /// <param name="errorProjector">Delegate that transforms the validation errors into the errors to throw</param>
        public static void ThrowOnValidationErrors<TEntity, TError>(this ValidationResult<TEntity, TError> validationResult, Func<ValidationError<TError>, string> errorProjector)
        {
            validationResult.ValidateArgument(nameof(validationResult));
            errorProjector.ValidateArgument(nameof(errorProjector));

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(x => errorProjector(x));
                errors.ThrowOnValidationErrors(validationResult.Validated);
            }
        }
    }
}
