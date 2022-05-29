using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions;

namespace Sels.Core.Localization
{
    /// <summary>
    /// Thrown when a localization entry for a culture is missing.
    /// </summary>
    public class LocalizationMissingException : Exception
    {
        // Constants
        private const string MessageFormat = "Localization with key {0} for culture {1} is missing";

        // Properties
        /// <summary>
        /// The key of the localization entry that was missing.
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// The culture the localization entry was missing for.
        /// </summary>
        public string Culture { get; }

        /// <inheritdoc cref="LocalizationMissingException"/>
        /// <param name="key"><inheritdoc cref="Key"/></param>
        /// <param name="culture"><inheritdoc cref="Culture"/></param>
        public LocalizationMissingException(string key, string culture) : base(MessageFormat.FormatString(key.ValidateArgumentNotNullOrWhitespace(nameof(key)), culture.ValidateArgumentNotNullOrWhitespace(nameof(culture))))
        {

        }
    }
}
