using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Bootstrap.V5.Components
{
    /// <summary>
    /// Contains the bootstrap button colors.
    /// </summary>
    public enum ButtonColor
    {
        /// <summary>
        /// No button color.
        /// </summary>
        [StringEnumValue("")]
        None = 0,
        /// <summary>
        /// The primary button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.Button + "-" + Bootstrap.Color.Primary)]
        Primary,
        /// <summary>
        /// The secondary button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.Button + "-" + Bootstrap.Color.Secondary)]
        Secondary,
        /// <summary>
        /// The success button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.Button + "-" + Bootstrap.Color.Success)]
        Success,
        /// <summary>
        /// The danger button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.Button + "-" + Bootstrap.Color.Danger)]
        Danger,
        /// <summary>
        /// The warning button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.Button + "-" + Bootstrap.Color.Warning)]
        Warning,
        /// <summary>
        /// The info button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.Button + "-" + Bootstrap.Color.Info)]
        Info,
        /// <summary>
        /// The light button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.Button + "-" + Bootstrap.Color.Light)]
        Light,
        /// <summary>
        /// The dark button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.Button + "-" + Bootstrap.Color.Dark)]
        Dark,
        /// <summary>
        /// The link button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.Button + "-link")]
        Link,
        /// <summary>
        /// The outline primary button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.ButtonOutline + "-" + Bootstrap.Color.Primary)]
        PrimaryOutline,
        /// <summary>
        /// The outline secondary button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.ButtonOutline + "-" + Bootstrap.Color.Secondary)]
        SecondaryOutline,
        /// <summary>
        /// The outline success button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.ButtonOutline + "-" + Bootstrap.Color.Success)]
        SuccessOutline,
        /// <summary>
        /// The outline danger button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.ButtonOutline + "-" + Bootstrap.Color.Danger)]
        DangerOutline,
        /// <summary>
        /// The outline warning button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.ButtonOutline + "-" + Bootstrap.Color.Warning)]
        WarningOutline,
        /// <summary>
        /// The outline info button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.ButtonOutline + "-" + Bootstrap.Color.Info)]
        InfoOutline,
        /// <summary>
        /// The outline light button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.ButtonOutline + "-" + Bootstrap.Color.Light)]
        LightOutline,
        /// <summary>
        /// The outline dark button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.ButtonOutline + "-" + Bootstrap.Color.Dark)]
        DarkOutline,
        /// <summary>
        /// The outline link button style.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.ButtonOutline + "-link")]
        LinkOutline
    }
    /// <summary>
    /// Static extension methods for <see cref="ButtonColor"/>.
    /// </summary>
    public static class ButtonTypeExtensions
    {
        /// <summary>
        /// Gets the css class for <paramref name="buttonType"/>.
        /// </summary>
        /// <param name="buttonType">Button color to get the css class for</param>
        /// <returns>The css class for <paramref name="buttonType"/></returns>
        public static string ToCss(this ButtonColor buttonType)
        {
            return buttonType.GetStringValue();
        }

        /// <summary>
        /// Gets the css class for a button color matching <paramref name="color"/>.
        /// </summary>
        /// <param name="color">The color to translate into a button color</param>
        /// <param name="isOutline">If the color needs to be an outline button color</param>
        /// <returns>Button color css where the color is equal to <paramref name="color"/></returns>
        public static string ToButtonCss(this Color color, bool isOutline = false)
        {
            return $"{(isOutline ? Bootstrap.Components.ButtonOutline : Bootstrap.Components.Button)}-{color.ToCss()}";
        }
    }
}
