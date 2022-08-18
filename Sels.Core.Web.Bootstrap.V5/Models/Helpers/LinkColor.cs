using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Bootstrap.V5.Helpers
{
    /// <summary>
    /// The bootstrap link colors for a elements.
    /// </summary>
    public enum LinkColor
    {
        /// <summary>
        /// The default link color.
        /// </summary>
        [StringEnumValue("")]
        Default = 0,
        /// <summary>
        /// The primary link color.
        /// </summary>
        [StringEnumValue(Bootstrap.Helpers.Link + "-" + Bootstrap.Color.Primary)]
        Primary,
        /// <summary>
        /// The secondary link color.
        /// </summary>
        [StringEnumValue(Bootstrap.Helpers.Link + "-" + Bootstrap.Color.Secondary)]
        Secondary,
        /// <summary>
        /// The success link color.
        /// </summary>
        [StringEnumValue(Bootstrap.Helpers.Link + "-" + Bootstrap.Color.Success)]
        Success,
        /// <summary>
        /// The danger link color.
        /// </summary>
        [StringEnumValue(Bootstrap.Helpers.Link + "-" + Bootstrap.Color.Danger)]
        Danger,
        /// <summary>
        /// The warning link color.
        /// </summary>
        [StringEnumValue(Bootstrap.Helpers.Link + "-" + Bootstrap.Color.Warning)]
        Warning,
        /// <summary>
        /// The info link color.
        /// </summary>
        [StringEnumValue(Bootstrap.Helpers.Link + "-" + Bootstrap.Color.Info)]
        Info,
        /// <summary>
        /// The light link color.
        /// </summary>
        [StringEnumValue(Bootstrap.Helpers.Link + "-" + Bootstrap.Color.Light)]
        Light,
        /// <summary>
        /// The dark link color.
        /// </summary>
        [StringEnumValue(Bootstrap.Helpers.Link + "-" + Bootstrap.Color.Dark)]
        Dark,
    }

    /// <summary>
    /// Static extension methods for <see cref="LinkColor"/>.
    /// </summary>
    public static class LinkColorExtensions
    {
        /// <summary>
        /// Gets the css class for <paramref name="linkColor"/>.
        /// </summary>
        /// <param name="linkColor">Link color to get the css class for</param>
        /// <returns>The css class for <paramref name="linkColor"/></returns>
        public static string ToCss(this LinkColor linkColor)
        {
            return linkColor.GetStringValue();
        }

        /// <summary>
        /// Gets the css class for a link color matching <paramref name="color"/>.
        /// </summary>
        /// <param name="color">The color to translate into a link color</param>
        /// <returns>Link color css where the color is equal to <paramref name="color"/></returns>
        public static string ToLinkCss(this Color color)
        {
            return $"{Bootstrap.Helpers.Link}-{color.ToCss()}";
        }
    }
}
