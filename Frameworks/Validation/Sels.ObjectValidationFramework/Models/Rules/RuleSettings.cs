using System;

namespace Sels.ObjectValidationFramework.Rules
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
        /// Ignore any exceptions throw by the rule. Rule will return valid instead.
        /// </summary>
        IgnoreExceptions = 1
    }
}
