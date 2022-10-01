using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Localization
{
    /// <summary>
    /// Exposes extra options when fetching localizations.
    /// </summary>
    public record LocalizationOptions
    {
        /// <summary>
        /// The default culture to search for when either no culture is provided when fetching or when a localization entry is missing for the requested culture. 
        /// When set to null the culture will not be searched.
        /// </summary>
        public string? DefaultCulture { get; set; }
        /// <summary>
        /// What to return if a key could not be localized.
        /// </summary>
        public MissingLocalizationSettings MissingKeySettings { get; set; } = MissingLocalizationSettings.Default;
    }
}
