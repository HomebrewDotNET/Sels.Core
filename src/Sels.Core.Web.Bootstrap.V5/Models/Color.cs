using Sels.Core.Attributes.Enumeration.Value;

namespace Sels.Core.Web.Bootstrap
{
    /// <summary>
    /// Contains the bootstrap colors re-used by other components.
    /// </summary>
    public enum Color
    {
        /// <summary>
        /// No color selected.
        /// </summary>
        [StringEnumValue("")]
        None,
        /// <summary>
        /// The primary color.
        /// </summary>
        [StringEnumValue(Bootstrap.Color.Primary)]
        Primary,
        /// <summary>
        /// The secondary color.
        /// </summary>
        [StringEnumValue(Bootstrap.Color.Secondary)]
        Secondary,
        /// <summary>
        /// The success color.
        /// </summary>
        [StringEnumValue(Bootstrap.Color.Success)]
        Success,
        /// <summary>
        /// The primary color.
        /// </summary>
        [StringEnumValue(Bootstrap.Color.Danger)]
        Danger,
        /// <summary>
        /// The warning color.
        /// </summary>
        [StringEnumValue(Bootstrap.Color.Warning)]
        Warning,
        /// <summary>
        /// The info color.
        /// </summary>
        [StringEnumValue(Bootstrap.Color.Info)]
        Info,
        /// <summary>
        /// The light color.
        /// </summary>
        [StringEnumValue(Bootstrap.Color.Light)]
        Light,
        /// <summary>
        /// The dark color.
        /// </summary>
        [StringEnumValue(Bootstrap.Color.Dark)]
        Dark
    }

    /// <summary>
    /// Static extension methods for <see cref="Color"/>.
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Gets the css class for <paramref name="color"/>.
        /// </summary>
        /// <param name="color">Color to get the css class for</param>
        /// <returns>The css class for <paramref name="color"/></returns>
        public static string ToCss(this Color color)
        {
            return color.GetStringValue();
        }
    }
}
