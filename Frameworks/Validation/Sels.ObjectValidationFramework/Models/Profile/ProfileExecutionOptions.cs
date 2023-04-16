using Sels.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.ObjectValidationFramework.Profile
{
    /// <summary>
    /// Modifies the behaviour how profiles execute validation.
    /// </summary>
    [Flags]
    public enum ProfileExecutionOptions
    {
        /// <summary>
        /// No selected options.
        /// </summary>
        None = 0,
        /// <summary>
        /// If the profile should throw a <see cref="ValidationException{TEntity, TError}"/> instead of returning the errors if there are any.
        /// </summary>
        ThrowOnError = 1,
        /// <summary>
        /// The profile won't go through the properties of the root object disabling the validation of the full hierarchy.
        /// </summary>
        NoPropertyFallthrough = 2,
        /// <summary>
        /// The profile won't loop over detected collections and validate the elements.
        /// </summary>
        NoCollectionFallthrough = 4,
        /// <summary>
        /// If the root object being validated is a collection the profile won't validate the elements.
        /// </summary>
        NoRootCollectionFallthrough = 8
    }
}
