using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Bootstrap.Components
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
        Default = 0,
        /// <summary>
        /// The small modal size of 300px.
        /// </summary>
        [StringEnumValue("modal-sm")]
        Small = 1,
        /// <summary>
        /// The large modal size of 800px.
        /// </summary>
        [StringEnumValue("modal-lg")]
        Large = 2,
        /// <summary>
        /// The extra large modal size of 1140px.
        /// </summary>
        [StringEnumValue("modal-xl")]
        ExtraLarge = 3
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
