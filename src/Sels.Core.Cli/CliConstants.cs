namespace Sels.Core.Cli
{
    /// <summary>
    /// Contains constant values.
    /// </summary>
    public class CliConstants
    {
        /// <summary>
        /// Contains constant values for argument parsing.
        /// </summary>
        public class ArgumentParsing
        {
            /// <summary>
            /// The prefix for defining values in a pattern.
            /// </summary>
            public const string PatternPrefix = "{";
            /// <summary>
            /// The suffix for defining values in a pattern.
            /// </summary>
            public const string PatternSuffix = "}";
            /// <summary>
            /// The Option value for the option pattern.
            /// </summary>
            public const string OptionPatternOptionName = PatternPrefix + "Option" + PatternSuffix;
            /// <summary>
            /// The Arg value for the option pattern.
            /// </summary>
            public const string OptionPatternArgName = PatternPrefix + "Arg" + PatternSuffix;
            /// <summary>
            /// The Key value for the option pattern.
            /// </summary>
            public const string KeyValuePatternKeyName = PatternPrefix + "Key" + PatternSuffix;
            /// <summary>
            /// The Value value for the option pattern.
            /// </summary>
            public const string KeyValuePatternValueName = PatternPrefix + "Value" + PatternSuffix;
        }
    }
}
