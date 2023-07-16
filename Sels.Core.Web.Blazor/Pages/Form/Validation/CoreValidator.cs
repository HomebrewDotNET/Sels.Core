using Microsoft.AspNetCore.Components;
using Sels.Core.Validation;
using Microsoft.AspNetCore.Components.Forms;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Web.Blazor.Exceptions;

namespace Sels.Core.Web.Blazor.Pages.Form.Validation
{
    /// <summary>
    /// Validator for an edit form that delegates validation to <see cref="IValidator{TEntity, TError, TContext}"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of objects to validate</typeparam>
    /// <typeparam name="TError">Type of validation errors to returns</typeparam>
    /// <typeparam name="TContext">Type of optional context to modify the behaviour of this validator</typeparam>
    public class CoreContextValidator<TEntity, TError, TContext> : ValidationCleaner
    {
        // Properties
        /// <summary>
        /// The validator that will be used to validate the form model
        /// </summary>
        [Inject]
        public IValidator<TEntity, TError, TContext> Validator { get; set; }
        /// <summary>
        /// Optional delegate for getting the error message to display selected from <typeparamref name="TError"/>. Default is <see cref="object.ToString()"/>.
        /// </summary>
        [Parameter]
        public Func<TError, string>? GetErrorMessage { get; set; }
        /// <summary>
        /// Optional delegate that returns the name of the field that the error is for. If null then the error message will be added to the global field.
        /// </summary>
        [Parameter]
        public Func<TError, string?> GetFieldForError { get; set; }
        /// <summary>
        /// Optional delegate that returns the validation context supplied to the validator.
        /// </summary>
        [Parameter]
        public Func<TEntity, TContext> GetValidationContext { get; set; }

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            MissingCascadingParameterException.ThrowOnNull(this, Context);
            Context.OnValidationRequested += HandleValidation;
        }

        private void HandleValidation(object? sender, ValidationRequestedEventArgs args)
        {
            args.ValidateArgument(nameof(args));
            MessageStore.Clear();

            var model = Context.Model.CastTo<TEntity>();
            var errors = Validator.Validate(model, GetValidationContext != null ? GetValidationContext(model) : default);

            if (errors != null)
            {
                foreach (var error in errors)
                {
                    var fieldName = (GetFieldForError != null ? GetFieldForError(error) : null) ?? GlobalFieldName;
                    var message = GetErrorMessage != null ? GetErrorMessage(error) : error.ToString();

                    MessageStore.Add(Context.Field(fieldName), message);
                }
            }

            Context.NotifyValidationStateChanged();
        }
    }

    /// <summary>
    /// Validator for an edit form that delegates validation to <see cref="IValidator{TEntity, TError}"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of objects to validate</typeparam>
    /// <typeparam name="TError">Type of validation errors to returns</typeparam>
    public class CoreValidator<TEntity, TError> : CoreContextValidator<TEntity, TError, object>
    {
    }
}
