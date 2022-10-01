using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Localization
{
    /// <summary>
    /// What should be returned if a key could not be localized.
    /// </summary>
    public enum MissingLocalizationSettings
    {
        /// <summary>
        /// Return a default value based on the key.
        /// </summary>
        Default,
        /// <summary>
        /// Return the key.
        /// </summary>
        Key,
        /// <summary>
        /// Throw a <see cref="LocalizationMissingException"/>.
        /// </summary>
        Exception,
        /// <summary>
        /// Return null.
        /// </summary>
        Null
    }
}
