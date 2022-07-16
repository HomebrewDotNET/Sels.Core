using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Contracts.Validation;
using Microsoft.AspNetCore.Components.Forms;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Web.Blazor.Exceptions;

namespace Sels.Core.Web.Blazor.Pages.Form
{
    /// <summary>
    /// Validator for an edit form that delegates validation to <see cref="IValidator{TEntity, TError, TContext}"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of objects to validate</typeparam>
    /// <typeparam name="TError">Type of validation errors to returns</typeparam>
    /// <typeparam name="TContext">Type of optional context to modify the behaviour of this validator</typeparam>
    public class CoreContextValidator<TEntity, TError, TContext> : ComponentBase
    {
        // Constants
        /// <summary>
        /// The name of the field for validation messages not tied to a field.
        /// </summary>
        public const string GlobalFieldName = BlazorConstants.Form.GlobalFieldName;

        // Fields
        private ValidationMessageStore? _messageStore;

        // Properties
        /// <summary>
        /// The validator that will be used to validate the form model
        /// </summary>
        [Inject]
        public IValidator<TEntity, TError, TContext> Validator { get; set; }
        /// <summary>
        /// The edit context of the parent component.
        /// </summary>
        [CascadingParameter]
        public EditContext Context { get; set; }
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
            _messageStore = new ValidationMessageStore(Context);

            Context.OnValidationRequested += (s, e) =>
            {
                _messageStore?.Clear();
                Context.NotifyValidationStateChanged();
            };
                
            Context.OnFieldChanged += (s, e) =>
            {
                _messageStore?.Clear(e.FieldIdentifier);
                // Clear global
                _messageStore.Clear(Context.Field(GlobalFieldName));
                Context.NotifyValidationStateChanged();
            };
            Context.OnValidationRequested += HandleValidation;
        }

        private void HandleValidation(object? sender, ValidationRequestedEventArgs args)
        {
            args.ValidateArgument(nameof(args));
            _messageStore.Clear();

            var model = Context.Model.Cast<TEntity>();
            var errors = Validator.Validate(model, GetValidationContext != null ? GetValidationContext(model) : default);

            if (errors != null)
            {
                foreach (var error in errors)
                {
                    var fieldName = (GetFieldForError != null ? GetFieldForError(error) : null) ?? GlobalFieldName;
                    var message = GetErrorMessage != null ? GetErrorMessage(error) : error.ToString();

                    _messageStore.Add(Context.Field(fieldName), message);
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
