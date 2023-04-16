using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Profile;

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
        /// <typeparam name="TEntity">The type of the entity yo validate</typeparam>
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <param name="objectToValidate">Object to validate</param>
        /// <param name="context">Optional object containing additional information for the validators</param>
        /// <returns>The validation result for <paramref name="objectToValidate"/></returns>
        public static ValidationResult<TEntity, TError> ValidateAgainstProfile<TProfile, TEntity, TError>(this TEntity objectToValidate, object context = null) where TProfile : ValidationProfile<TError>, new()
        {
            objectToValidate.ValidateArgument(nameof(objectToValidate));

            return new TProfile().Validate(objectToValidate, context);
        }
    }
}
