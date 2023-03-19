using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Sels.Core.Web.Blazor.Exceptions;

namespace Sels.Core.Web.Blazor.Pages.Form.Validation
{
    /// <summary>
    /// Removes markup from fields in an edit form when the input changes. Handy for custom async validation that is triggered in the submit.
    /// </summary>
    public class ValidationCleaner : ComponentBase
    {
        // Constants
        /// <summary>
        /// The name of the field for validation messages not tied to a field.
        /// </summary>
        public const string GlobalFieldName = BlazorConstants.Form.GlobalFieldName;

        // Properties
        /// <summary>
        /// The edit context of the parent component.
        /// </summary>
        [CascadingParameter]
        public EditContext Context { get; set; }
        /// <summary>
        /// The message store used by the validator.
        /// </summary>
        [Parameter]
        public ValidationMessageStore? MessageStore { get; set; }

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            MissingCascadingParameterException.ThrowOnNull(this, Context);
            MessageStore ??= new ValidationMessageStore(Context);

            Context.OnValidationRequested += (s, e) =>
            {
                MessageStore.Clear();
                Context.MarkAsUnmodified();
                Context.NotifyValidationStateChanged();
            };

            Context.OnFieldChanged += (s, e) =>
            {
                MessageStore?.Clear(e.FieldIdentifier);
                // Clear global
                MessageStore.Clear(Context.Field(GlobalFieldName));
                Context.NotifyValidationStateChanged();
            };
        }
    }
}
