using Newtonsoft.Json.Linq;
using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.ObjectValidationFramework.Profile
{
    /// <summary>
    /// Contains the result of validation triggered on a profile.
    /// </summary>
    /// <typeparam name="TEntity">The type of the root object that was validated</typeparam>
    /// <typeparam name="TError">The type of the error message</typeparam>
    public class ValidationResult<TEntity, TError>
    {
        /// <summary>
        /// The root object that validation was triggered on.
        /// </summary>
        public TEntity Validated { get; internal set; }
        /// <summary>
        /// Array with all objects that were checked by the profile in order.
        /// </summary>
        public object[] CallStack { get; internal set; }
        /// <summary>
        /// How many object instances were checked by the profile.
        /// </summary>
        public int Scanned => CallStack.Length;
        /// <summary>
        /// Any validation errors for <see cref="Validated"/> and it's children.
        /// </summary>
        public ValidationError<TError>[] Errors { get; internal set; }
        /// <summary>
        /// Returns all the validation errors messages from <see cref="Errors"/>.
        /// </summary>
        public IEnumerable<TError> Messages => Errors.Select(x => x.Message);
        /// <summary>
        /// True if <see cref="Validated"/> contains no errors, otherwise false.
        /// </summary>
        public bool IsValid => !Errors.HasValue();

        /// <summary>
        /// Changes the type of the current validation error messages to <typeparamref name="TNewError"/>.
        /// </summary>
        /// <typeparam name="TNewError">The type of the new error message</typeparam>
        /// <param name="projector">Delegate that converts the current error messages to the new type</param>
        /// <returns>Current results with <see cref="Errors"/> converted to <typeparamref name="TNewError"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ValidationResult<TEntity, TNewError> ChangeMessageTo<TNewError>(Func<ValidationError<TError>, TNewError> projector)
        {
            projector.ValidateArgument(nameof(projector));

            return new ValidationResult<TEntity, TNewError> {
                Validated = Validated,
                CallStack = CallStack,
                Errors = Errors.Select(x => x.ChangeMessageTo(projector)).ToArray()
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            if (IsValid)
            {
                builder.Append(typeof(TEntity).Name).Append(" is valid");
            }
            else
            {
                builder.Append(typeof(TEntity).Name).Append(" is not valid and contains ").Append(Errors.Length).Append(" errors:");
                foreach(var error in Errors)
                {
                    builder.AppendLine().Append(error.ToString());
                }
            }

            return builder.ToString();
        }
    }
}
