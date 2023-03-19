namespace Sels.Core.Cli.ArgumentParsing
{
    /// <summary>
    /// A null instance without any properties to use with a <see cref="IArgumentParser{T}"/> when no properties need to be parsed to.
    /// </summary>
    public class NullArguments
    {
        /// <inheritdoc cref="NullArguments"/>
        public NullArguments()
        {

        }
        /// <summary>
        /// Static instance to use.
        /// </summary>
        public static NullArguments Instance = new NullArguments();
    }
}
