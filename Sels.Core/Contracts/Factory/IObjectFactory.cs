using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Contracts.Factory
{
    /// <summary>
    /// Factory responsible for creating objects using an identifier and contruction arguments.
    /// </summary>
    public interface IObjectFactory
    {
        /// <summary>
        /// Creates a new object with Identifier <paramref name="identifier"/> and optional contructor arguments <paramref name="arguments"/>.
        /// </summary>
        /// <param name="identifier">Object idetifier so factory knows which object to create</param>
        /// <param name="arguments">Optiona arguments for creating the object</param>
        /// <returns>A new instance of object with id <paramref name="identifier"/></returns>
        object Build(string identifier, params object[] arguments);

        /// <summary>
        /// Creates a new object with Identifier <paramref name="identifier"/> and optional contructor arguments <paramref name="arguments"/> and casts it to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to cast created object to</typeparam>
        /// <param name="identifier">Object idetifier so factory knows which object to create</param>
        /// <param name="arguments">Optiona arguments for creating the object</param>
        /// <returns>A new instance of object with id <paramref name="identifier"/> casted to <typeparamref name="T"/></returns>
        T Build<T>(string identifier, params object[] arguments);

        /// <summary>
        /// Creates a new object using the configuration section with Name <paramref name="section"/>.
        /// </summary>
        /// <param name="section">Name of section containing the build configuration</param>
        /// <param name="parentSections">Optional parent sections</param>
        /// <returns>A new instance of the object defined in config</returns>
        object BuildFromConfig(string section, params string[] parentSections);

        /// <summary>
        /// Creates a new object  casted to <typeparamref name="T"/> using the configuration section with Name <paramref name="section"/>.
        /// </summary>
        /// <param name="section">Name of section containing the build configuration</param>
        /// <param name="parentSections">Optional parent sections</param>
        /// <returns>A new instance of the object defined in config casted to <typeparamref name="T"/></returns>
        T BuildFromConfig<T>(string section, params string[] parentSections);

        /// <summary>
        /// Creates new objects casted to <typeparamref name="T"/> using the configuration section with Name <paramref name="section"/>.
        /// </summary>
        /// <param name="section">Name of section containing the build configuration</param>
        /// <param name="parentSections">Optional parent sections</param>
        /// <returns>A new instance of the object defined in config</returns>
        object[] BuildAllFromConfig(string section, params string[] parentSections);

        /// <summary>
        /// Creates new objects casted to <typeparamref name="T"/> using the configuration section with Name <paramref name="section"/>.
        /// </summary>
        /// <param name="section">Name of section containing the build configuration</param>
        /// <param name="parentSections">Optional parent sections</param>
        /// <returns>A new instance of the object defined in config casted to <typeparamref name="T"/></returns>
        T[] BuildAllFromConfig<T>(string section, params string[] parentSections);
    }
}
