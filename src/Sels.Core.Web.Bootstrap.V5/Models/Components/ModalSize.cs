using Sels.Core.Attributes.Enumeration.Value;

namespace Sels.Core.Web.Bootstrap.V5.Components
{
    /// <summary>
    /// The sizes for a bootstrap modal.
    /// </summary>
    public enum ModalSize
    {
        /// <summary>
        /// The default modal size of 500px.
        /// </summary>
        [StringEnumValue("")]
        Default,
        /// <summary>
        /// The small modal size of 300px.
        /// </summary>
        [StringEnumValue("modal-sm")]
        Small,
        /// <summary>
        /// The large modal size of 800px.
        /// </summary>
        [StringEnumValue("modal-lg")]
        Large,
        /// <summary>
        /// The extra large modal size of 1140px.
        /// </summary>
        [StringEnumValue("modal-xl")]
        ExtraLarge
    }

    /// <summary>
    /// Static extension methods for <see cref="ModalSize"/>.
    /// </summary>
    public static class ModalSizeExtensions
    {
        /// <summary>
        /// Gets the css class for <paramref name="modalSize"/>.
        /// </summary>
        /// <param name="modalSize">Modal size to get the css class for</param>
        /// <returns>The css class for <paramref name="modalSize"/></returns>
        public static string ToCss(this ModalSize modalSize)
        {
            return modalSize.GetStringValue();
        }
    }
}
