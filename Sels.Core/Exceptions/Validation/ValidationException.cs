using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Validation
{
    /// <summary>
    /// Thrown when an entity of <typeparamref name="TEntity"/> contained validation errors.
    /// </summary>
    /// <typeparam name="TEntity">Type of the validated entity</typeparam>
    /// <typeparam name="TError">Type of the validation error</typeparam>
    public class ValidationException<TEntity, TError> : Exception
    {
        // Properties
        /// <summary>
        /// The entity that failed the validation.
        /// </summary>
        public TEntity Entity { get; }
        /// <summary>
        /// The validation errors for <see cref="Entity"/>
        /// </summary>
        public TError[] Errors { get; set; }

        /// <inheritdoc cref="ValidationException{TEntity, TError}"/>
        /// <param name="entity"><inheritdoc cref="Entity"/></param>
        /// <param name="errors"><inheritdoc cref="Errors"/></param>
        /// <param name="entityName">Optional display name for <paramref name="entity"/></param>
        public ValidationException(TEntity entity, IEnumerable<TError> errors, string entityName = null) : base(CreateMessage(entityName.HasValue() ? entityName : typeof(TEntity).Name, errors))
        {
            Entity = entity.ValidateArgument(nameof(entity));
            Errors = errors.ToArray();
        }

        private static string CreateMessage(string entityName, IEnumerable<TError> errors)
        {
            entityName.ValidateArgumentNotNullOrWhitespace(nameof(entityName));
            errors.ValidateArgumentNotNullOrEmpty(nameof(errors));

            return $"{entityName} was not valid: {Environment.NewLine}{errors.JoinStringNewLine()}";
        }
    }
}
