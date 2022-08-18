using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Bootstrap.V5.Utilities
{
    /// <summary>
    /// The bootstrap text colors.
    /// </summary>
    public enum TextColor
    {
        /// <summary>
        /// The default text color.
        /// </summary>
        [StringEnumValue("")]
        Default = 0,
        /// <summary>
        /// The primary text color.
        /// </summary>
        [StringEnumValue(Bootstrap.Utilities.Text + "-" + Bootstrap.Color.Primary)]
        Primary,
        /// <summary>
        /// The secondary text color.
        /// </summary>
        [StringEnumValue(Bootstrap.Utilities.Text + "-" + Bootstrap.Color.Secondary)]
        Secondary,
        /// <summary>
        /// The success text color.
        /// </summary>
        [StringEnumValue(Bootstrap.Utilities.Text + "-" + Bootstrap.Color.Success)]
        Success,
        /// <summary>
        /// The danger text color.
        /// </summary>
        [StringEnumValue(Bootstrap.Utilities.Text + "-" + Bootstrap.Color.Danger)]
        Danger,
        /// <summary>
        /// The warning text color.
        /// </summary>
        [StringEnumValue(Bootstrap.Utilities.Text + "-" + Bootstrap.Color.Warning)]
        Warning,
        /// <summary>
        /// The info text color.
        /// </summary>
        [StringEnumValue(Bootstrap.Utilities.Text + "-" + Bootstrap.Color.Info)]
        Info,
        /// <summary>
        /// The light text color.
        /// </summary>
        [StringEnumValue(Bootstrap.Utilities.Text + "-" + Bootstrap.Color.Light)]
        Light,
        /// <summary>
        /// The dark text color.
        /// </summary>
        [StringEnumValue(Bootstrap.Utilities.Text + "-" + Bootstrap.Color.Dark)]
        Dark,
        /// <summary>
        /// The body text color.
        /// </summary>
        [StringEnumValue(Bootstrap.Utilities.Text + "-body")]
        Body,
        /// <summary>
        /// The muted text color.
        /// </summary>
        [StringEnumValue(Bootstrap.Utilities.Text + "-muted")]
        Muted,
        /// <summary>
        /// The white text color.
        /// </summary>
        [StringEnumValue(Bootstrap.Utilities.Text + "-white")]
        White,
        /// <summary>
        /// The half black text color.
        /// </summary>
        [StringEnumValue(Bootstrap.Utilities.Text + "-black-50")]
        HalfBlack,
        /// <summary>
        /// The half white text color.
        /// </summary>
        [StringEnumValue(Bootstrap.Utilities.Text + "-white-50")]
        HalfWhite,
    }

    /// <summary>
    /// Static extension methods for <see cref="TextColor"/>.
    /// </summary>
    public static class TextColorExtensions
    {
        /// <summary>
        /// Gets the css class for <paramref name="textColor"/>.
        /// </summary>
        /// <param name="textColor">Text color to get the css class for</param>
        /// <returns>The css class for <paramref name="textColor"/></returns>
        public static string ToCss(this TextColor textColor)
        {
            return textColor.GetStringValue();
        }

        /// <summary>
        /// Gets the css class for a text color matching <paramref name="color"/>.
        /// </summary>
        /// <param name="color">The color to translate into a text color</param>
        /// <returns>Text color css where the color is equal to <paramref name="color"/></returns>
        public static string ToTextCss(this Color color)
        {
            return $"{Bootstrap.Utilities.Text}-{color.ToCss()}";
        }
    }
}
