namespace Sels.Core.Deployment.Parsing.Environment
{
    /// <summary>
    /// Thrown when an environment variable has an incorrect format and could not be parsed.
    /// </summary>
    public class CouldNotParseEnvironmentVariableException : Exception
    {
        // Properties
        /// <summary>
        /// The name of the environment variable.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The value of the variable.
        /// </summary>
        public string Value { get; }

        /// <inheritdoc cref="CouldNotParseEnvironmentVariableException"/>
        /// <param name="name"><inheritdoc cref="Name"/></param>
        /// <param name="value"><inheritdoc cref="Value"/></param>
        public CouldNotParseEnvironmentVariableException(string name, string value) : base(CreateMessage(name, value))
        {
            Name = name;
            Value = value;
        }

        private static string CreateMessage(string name, string value)
        {
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));
            value.ValidateArgumentNotNullOrWhitespace(nameof(value));

            return $"Environment variable {name} with value <{value}> has an incorrect format and could not be parsed";
        }
    }
}
