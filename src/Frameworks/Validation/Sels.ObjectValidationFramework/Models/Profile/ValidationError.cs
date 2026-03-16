using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Validators;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sels.ObjectValidationFramework.Profile
{
    /// <summary>
    /// Represents a validation errors returned by a profile.
    /// </summary>
    /// <typeparam name="TError">The type of the error message</typeparam>
    public class ValidationError<TError>
    {
        // Properties
        /// <summary>
        /// The validation error message.
        /// </summary>
        public TError Message { get; internal set; }
        /// <summary>
        /// The value that was validated.
        /// </summary>
        public object Value { get; internal set; }
        /// <summary>
        /// The source object <see cref="Value"/> is from.
        /// </summary>
        public object Source { get; internal set; }
        /// <summary>
        /// The property info if <see cref="Value"/> came from a property, otherwise null.
        /// </summary>
        public PropertyInfo Property { get; internal set; }
        /// <summary>
        /// Index of <see cref="Source"/> if it was part of a collection. Will be null if it wasn't part of a collection.
        /// </summary>
        public int? ElementIndex { get; internal set; }
        /// <summary>
        /// Hierarchy of object if property fallthrough is enabled. The previous element is always the parent of the next element. Will be empty if <see cref="Source"/> is the root object being validated.
        /// </summary>
        public Parent[] Parents { get; internal set; }
        /// <summary>
        /// Returns a display name of the value that was validated.
        /// </summary>
        public string DisplayName { get; internal set; }
        /// <summary>
        /// Returns a display name of the value that was validated. Includes the parent hierarchy.
        /// </summary>
        public string FullDisplayName { get; internal set; }

        /// <summary>
        /// Changes the type of the current validation error message to <typeparamref name="TNewError"/>.
        /// </summary>
        /// <typeparam name="TNewError">The type of the new error message</typeparam>
        /// <param name="projector">Delegate that converts the current error message to the new type</param>
        /// <returns>Current error with <see cref="Message"/> converted to <typeparamref name="TNewError"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ValidationError<TNewError> ChangeMessageTo<TNewError>(Func<ValidationError<TError>, TNewError> projector)
        {
            projector.ValidateArgument(nameof(projector));
            var newMessage = projector(this);
            if (newMessage == null) throw new InvalidOperationException($"Delegate returned null when changing error message from type <{typeof(TError)}> to <{typeof(TNewError)}>");
            return new ValidationError<TNewError>()
            {
                Message = newMessage,
                Value = Value,
                Source = Source,
                Property = Property,
                ElementIndex = ElementIndex,
                Parents = Parents,
                DisplayName = DisplayName,
                FullDisplayName = FullDisplayName
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{FullDisplayName}: {Message}";
        }
    }
}
