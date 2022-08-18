using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Bootstrap.V5.Models.Components
{
    /// <summary>
    /// Contains the different types of bootsrap spinners.
    /// </summary>
    public enum SpinnerType
    {
        /// <summary>
        /// The bootstrap border spinner.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.Spinners.Border)]
        Border,
        /// <summary>
        /// The bootsrap grow spinner.
        /// </summary>
        [StringEnumValue(Bootstrap.Components.Spinners.Grow)]
        Grow
    }

    /// <summary>
    /// Static extension methods for <see cref="SpinnerType"/>.
    /// </summary>
    public static class SpinnerTypeExtensions
    {
        /// <summary>
        /// Gets the css class for <paramref name="spinnerType"/>.
        /// </summary>
        /// <param name="spinnerType">Spinner type to get the css class for</param>
        /// <param name="isSmall">If css needs to be returned for a small spinner</param>
        /// <returns>The css class for <paramref name="spinnerType"/></returns>
        public static string ToCss(this SpinnerType spinnerType, bool isSmall = false)
        {
            var spinnerCss = spinnerType.GetStringValue();
            return isSmall ? $"{spinnerCss} {spinnerCss}-sm" : spinnerCss;
        }
    }
}
