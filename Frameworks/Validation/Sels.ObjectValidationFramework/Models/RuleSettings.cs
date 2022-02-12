using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.ObjectValidationFramework.Models
{
    /// <summary>
    /// Exposes extra settings for a validation rule.
    /// </summary>
    [Flags]
    public enum RuleSettings
    {
        /// <summary>
        /// No settings selected.
        /// </summary>
        None = 0,
        /// <summary>
        /// Ignore any exceptions throw by the rule. Rule will returns valid instead.
        /// </summary>
        IgnoreExceptions = 1
    }
}
