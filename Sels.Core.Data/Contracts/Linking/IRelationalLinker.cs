using System;
using System.Collections.Generic;

namespace Sels.Core.Data.Linking
{
    /// <summary>
    /// Links different objects together.
    /// </summary>
    public interface IRelationalLinker
    {
        /// <summary>
        /// Defines an object type that the current instance can link.
        /// </summary>
        /// <typeparam name="T">Type of the object that can be linked.</typeparam>
        /// <param name="idGetter">Delegate that returns the unique id of the object</param>
        /// <returns>Builder for configuring how to link <typeparamref name="T"/> to other objects</returns>
        IRelationalLinkBuilder<T> With<T>(Func<T, object> idGetter);
        /// <summary>
        /// Links together <paramref name="firstObject"/>, <paramref name="secondObject"/> and optionally <paramref name="additionalObjects"/>.
        /// </summary>
        /// <typeparam name="T">The type of the first object</typeparam>
        /// <param name="firstObject">The first object to link</param>
        /// <param name="secondObject">The second object to link</param>
        /// <param name="additionalObjects">AOptional additonal objects to link</param>
        /// <returns><paramref name="firstObject"/></returns>
        T Link<T>(T firstObject, object secondObject, params object[] additionalObjects);

        /// <summary>
        /// Gets all objects of type <typeparamref name="T"/> that the current instance has linked.
        /// </summary>
        /// <typeparam name="T">Type of the objects to fetch</typeparam>
        /// <returns>Enumerator returning all the linked objects</returns>
        IEnumerable<T> GetAll<T>();

        /// <summary>
        /// Gets all objects of <paramref name="type"/> that the current instance has linked.
        /// </summary>
        /// <returns>Enumerator returning all the linked objects</returns>
        IEnumerable<object> GetAll(Type type);
    }

    /// <summary>
    /// Builder on how to link object of type <typeparamref name="T"/> to other objects.
    /// </summary>
    /// <typeparam name="T">Type of the object to create configuration for</typeparam>
    public interface IRelationalLinkBuilder<T> : IRelationalLinker
    {
        /// <summary>
        /// Initializes an object of type <typeparamref name="T"/> the first time the linker sees the object.
        /// </summary>
        /// <param name="initializer">Action that initializes the supplied object</param>
        /// <returns>The current builder for method chaining</returns>
        IRelationalLinkBuilder<T> InitializeWith(Action<T> initializer);
        /// <summary>
        /// Defines a link from <typeparamref name="T"/> to <typeparamref name="TLink"/>.
        /// </summary>
        /// <typeparam name="TLink">The object to link <typeparamref name="T"/> to</typeparam>
        /// <param name="condition">The condition that dictates that 2 objects can be linked</param>
        /// <param name="linker">The delegate that links together the objects</param>
        /// <returns>The current builder for method chaining</returns>
        IRelationalLinkBuilder<T> LinkTo<TLink>(Func<T, TLink, bool> condition, Action<T, TLink> linker);
    }
}
