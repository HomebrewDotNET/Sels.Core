using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Bootstrap.Components
{
    /// <summary>
    /// Contains the bootstrap button types.
    /// </summary>
    public enum ButtonType
    {
        /// <summary>
        /// The primary button style.
        /// </summary>
        [StringEnumValue("btn-primary")]
        Primary,
        /// <summary>
        /// The secondary button style.
        /// </summary>
        [StringEnumValue("btn-secondary")]
        Secondary,
        /// <summary>
        /// The success button style.
        /// </summary>
        [StringEnumValue("btn-success")]
        Success,
        /// <summary>
        /// The danger button style.
        /// </summary>
        [StringEnumValue("btn-danger")]
        Danger,
        /// <summary>
        /// The warning button style.
        /// </summary>
        [StringEnumValue("btn-warning")]
        Warning,
        /// <summary>
        /// The info button style.
        /// </summary>
        [StringEnumValue("btn-info")]
        Info,
        /// <summary>
        /// The light button style.
        /// </summary>
        [StringEnumValue("btn-light")]
        Light,
        /// <summary>
        /// The dark button style.
        /// </summary>
        [StringEnumValue("btn-dark")]
        Dark,
        /// <summary>
        /// The link button style.
        /// </summary>
        [StringEnumValue("btn-link")]
        Link,
        /// <summary>
        /// The outline primary button style.
        /// </summary>
        [StringEnumValue("btn-outline-primary")]
        PrimaryOutline,
        /// <summary>
        /// The outline secondary button style.
        /// </summary>
        [StringEnumValue("btn-outline-secondary")]
        SecondaryOutline,
        /// <summary>
        /// The outline success button style.
        /// </summary>
        [StringEnumValue("btn-outline-success")]
        SuccessOutline,
        /// <summary>
        /// The outline danger button style.
        /// </summary>
        [StringEnumValue("btn-outline-danger")]
        DangerOutline,
        /// <summary>
        /// The outline warning button style.
        /// </summary>
        [StringEnumValue("btn-outline-warning")]
        WarningOutline,
        /// <summary>
        /// The outline info button style.
        /// </summary>
        [StringEnumValue("btn-outline-info")]
        InfoOutline,
        /// <summary>
        /// The outline light button style.
        /// </summary>
        [StringEnumValue("btn-outline-light")]
        LightOutline,
        /// <summary>
        /// The outline dark button style.
        /// </summary>
        [StringEnumValue("btn-outline-dark")]
        DarkOutline,
        /// <summary>
        /// The outline link button style.
        /// </summary>
        [StringEnumValue("btn-outline-link")]
        LinkOutline
    }
    /// <summary>
    /// Static extension methods for <see cref="ButtonType"/>.
    /// </summary>
    public static class ButtonTypeExtensions
    {
        /// <summary>
        /// Gets the css class for <paramref name="buttonType"/>.
        /// </summary>
        /// <param name="buttonType">Button type to get the css class for</param>
        /// <returns>The css class for <paramref name="buttonType"/></returns>
        public static string ToCss(this ButtonType buttonType)
        {
            return buttonType.GetStringValue();
        }
    }
}
