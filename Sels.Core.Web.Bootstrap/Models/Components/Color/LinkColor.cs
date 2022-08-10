using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Bootstrap.Components.Color
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
        [StringEnumValue("link-primary")]
        Primary,
        /// <summary>
        /// The secondary link color.
        /// </summary>
        [StringEnumValue("link-secondary")]
        Secondary,
        /// <summary>
        /// The success link color.
        /// </summary>
        [StringEnumValue("link-success")]
        Success,
        /// <summary>
        /// The danger link color.
        /// </summary>
        [StringEnumValue("link-danger")]
        Danger,
        /// <summary>
        /// The warning link color.
        /// </summary>
        [StringEnumValue("link-warning")]
        Warning,
        /// <summary>
        /// The info link color.
        /// </summary>
        [StringEnumValue("link-info")]
        Info,
        /// <summary>
        /// The light link color.
        /// </summary>
        [StringEnumValue("link-light")]
        Light,
        /// <summary>
        /// The dark link color.
        /// </summary>
        [StringEnumValue("link-dark")]
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
    }
}
