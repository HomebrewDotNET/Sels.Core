using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sels.Core.Validation
{
    /// <summary>
    /// Thrown when an entity of <typeparamref name="TEntity"/> contained validation errors.
    /// </summary>
    /// <typeparam name="TEntity">Type of the validated entity</typeparam>
    /// <typeparam name="TError">Type of the validation error</typeparam>
    public class ValidationException<TEntity, TError> : ValidationException
    {
        // Properties
        /// <summary>
        /// The entity that failed the validation.
        /// </summary>
        public new TEntity Entity { get; }
        /// <summary>
        /// The validation errors for <see cref="Entity"/>
        /// </summary>
        public new TError[] Errors { get; }

        /// <inheritdoc cref="ValidationException{TEntity, TError}"/>
        /// <param name="entity"><inheritdoc cref="Entity"/></param>
        /// <param name="errors"><inheritdoc cref="Errors"/></param>
        /// <param name="entityName">Optional display name for <paramref name="entity"/></param>
        public ValidationException(TEntity entity, IEnumerable<TError> errors, string entityName = null) : base(entity, errors?.Select(x => x.ToString()), entityName.HasValue() ? entityName : typeof(TEntity).Name)
        {
            Entity = entity.ValidateArgument(nameof(entity));
            Errors = errors.ValidateArgumentNotNullOrEmpty(nameof(errors)).ToArray();
        }
    }

    /// <summary>
    /// Thrown when an entity contained validation errors.
    /// </summary>
    public class ValidationException : Exception
    {
        // Properties
        /// <summary>
        /// The entity that failed the validation.
        /// </summary>
        public object Entity { get; }
        /// <summary>
        /// The validation errors for <see cref="Entity"/>
        /// </summary>
        public string[] Errors { get; }

        /// <inheritdoc cref="ValidationException{TEntity, TError}"/>
        /// <param name="entity"><inheritdoc cref="Entity"/></param>
        /// <param name="errors"><inheritdoc cref="Errors"/></param>
        /// <param name="entityName">Optional display name for <paramref name="entity"/></param>
        public ValidationException(object entity, IEnumerable<string> errors, string entityName = null) : base(CreateMessage(entityName.HasValue() ? entityName : entity?.GetType()?.Name, errors))
        {
            Entity = entity.ValidateArgument(nameof(entity));
            Errors = errors.ValidateArgumentNotNullOrEmpty(nameof(errors)).ToArray();
        }

        private static string CreateMessage(string entityName, IEnumerable<string> errors)
        {
            entityName.ValidateArgumentNotNullOrWhitespace(nameof(entityName));
            errors.ValidateArgumentNotNullOrEmpty(nameof(errors));

            return $"{entityName} was not valid: {Environment.NewLine}{errors.JoinStringNewLine()}";
        }
    }
}
