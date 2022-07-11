using Sels.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Extensions.Validation
{
    /// <summary>
    /// Contains extension methods for validating objects.
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Throws a <see cref="ValidationException{TEntity, TError}"/> if <paramref name="errors"/> is not empty.
        /// </summary>
        /// <typeparam name="TEntity">Type of the entity that was validated</typeparam>
        /// <typeparam name="TError">Type of the validation error</typeparam>
        /// <param name="errors">Enumerator containing the validation errors, can be null</param>
        /// <param name="entity">The validated entity</param>
        /// <param name="entityName">Optional display name for <paramref name="entity"/></param>
        /// <exception cref="ValidationException{TEntity, TError}"></exception>
        public static void ThrowOnValidationErrors<TEntity, TError>(this IEnumerable<TError> errors, TEntity entity, string entityName = null)
        {
            entity.ValidateArgument(nameof(entity));

            if (errors.HasValue()) throw new ValidationException<TEntity, TError>(entity, errors, entityName);
        }
    }
}
