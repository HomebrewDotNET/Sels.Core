using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Templates.Profile;
using System;
using System.Collections.Generic;
using System.Text;

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
        /// <typeparam name="TError">Type of validation error to return</typeparam>
        /// <param name="objectToValidate">Object to validate</param>
        /// <param name="context">Type of optional context to moddify the behaviour of this validator</param>
        /// <returns>All validation errors for <paramref name="objectToValidate"/></returns>
        public static TError[] Validate<TProfile, TError>(this object objectToValidate, object context = null) where TProfile : ValidationProfile<TError>, new()
        {
            objectToValidate.ValidateArgument(nameof(objectToValidate));

            return new TProfile().Validate(objectToValidate, context);
        }
    }
}
