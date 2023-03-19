namespace Sels.Core.Web.Blazor.Pages.PopUp
{
    /// <summary>
    /// Allows modal content to interact with the modal itself.
    /// </summary>
    public class ModalContext
    {
        // Fields
        private readonly Modal _modal;

        /// <inheritdoc cref="ModalContext"/>
        /// <param name="modal">The modal to create the context for</param>
        public ModalContext(Modal modal)
        {
            _modal = modal.ValidateArgument(nameof(modal));
        }
        /// <inheritdoc cref="Modal.CloseAsync"/>
        public Task CloseAsync()
        {
            return _modal.CloseAsync();
        }
    }
}
