namespace Sels.Core.Factory
{
    /// <summary>
    /// Factory that is able to create new instances of <typeparamref name="T"/> based on supplied arguments.
    /// </summary>
    /// <typeparam name="T">Type of new instances that factory can create</typeparam>
    public interface IFactory<T>
    {
        /// <summary>
        /// Creates a new instance of <paramref name="arguments"/>.
        /// </summary>
        /// <param name="arguments">Optional arguments to create a new instance of <typeparamref name="T"/></param>
        /// <returns>A new instance of <typeparamref name="T"/></returns>
        T Create(params object[] arguments);
    }
}
