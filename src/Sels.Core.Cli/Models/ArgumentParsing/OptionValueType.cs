using System.Collections;

namespace Sels.Core.Cli.Contracts.ArgumentParsing
{
    /// <summary>
    /// Dictates how an option can be provided.
    /// </summary>
    public enum OptionValueType
    {
        /// <summary>
        /// Option type is inferred from the argument type. <see cref="bool"/> will result in <see cref="None"/>, a type assignable to <see cref="IEnumerable"/> will result in <see cref="List"/> and the default is <see cref="Single"/>.
        /// </summary>
        Default = 0,
        /// <summary>
        /// When the option is provided without any value like -h or --help.
        /// </summary>
        None = 1,
        /// <summary>
        /// When the option is provided with a value like -a Build or --action Build.
        /// </summary>
        Single = 2,
        /// <summary>
        /// When the option is provided with multiple values like -a Clean Build Publish or --action Clean Build Publish.
        /// </summary>
        List = 3 
    }
}
