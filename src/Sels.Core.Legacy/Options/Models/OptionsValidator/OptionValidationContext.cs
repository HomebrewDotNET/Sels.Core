namespace Sels.Core.Options
{
    /// <summary>Context passed to validators of named options.</summary>
    public class OptionValidationContext
    {
        /// <summary>The name of the option being validated, or an empty string for default options.</summary>
        public string Name { get; }

        /// <summary>Indicates whether the default option is being validated.</summary>
        public bool IsDefault => Name == string.Empty;

        /// <summary>Creates a validation context for the named option.</summary>
        /// <param name="name">The option name; null is normalized to an empty string.</param>
        public OptionValidationContext(string name)
        {
            Name = name ?? string.Empty;
        }
    }
}