using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Blazor.Models.Localization
{
    /// <summary>
    /// Context that contains localization info that can be provided to child components as cascading value.
    /// </summary>
    public class LocalizationContext
    {
        /// <summary>
        /// The culture to localize in.
        /// </summary>
        public CultureInfo Culture { get; set; }

        /// <inheritdoc cref="LocalizationContext"/>
        /// <param name="culture">The name of the culture to localize in</param>
        public LocalizationContext(string culture) : this(new CultureInfo(culture.ValidateArgumentNotNullOrWhitespace(nameof(culture))))
        {

        }

        /// <inheritdoc cref="LocalizationContext"/>
        /// <param name="culture"><inheritdoc cref="Culture"/></param>
        public LocalizationContext(CultureInfo culture)
        {
            Culture = culture.ValidateArgument(nameof(culture));
        }
    }
}
