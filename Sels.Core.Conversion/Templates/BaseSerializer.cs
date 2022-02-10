using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Conversion.Templates
{
    /// <summary>
    /// Template for creating serializers. Exposes type handlers for serializing/deserializing certain types in a different way.
    /// </summary>
    /// <typeparam name="TSerializerSource">The type to serialize to</typeparam>
    /// <typeparam name="TDeserializerSource">The type to deserialze from</typeparam>
    public abstract class BaseSerializer<TSerializerSource, TDeserializerSource>
    {
        // Fields
        /// <summary>
        /// Optional loggers for tracing.
        /// </summary>
        protected IEnumerable<ILogger> _loggers;
        private List<TypeHandler> _typeHandlers = new List<TypeHandler>();

        /// <summary>
        /// Adds a new type handler configured through <paramref name="configurator"/>. The type handler will be used when calling either <see cref="SerializeFrom(object)"/> or <see cref="DeserializeTo(TDeserializerSource, object)"/>.
        /// </summary>
        /// <param name="configurator">The delegate to configure the new type handler</param>
        protected void AddTypeHandler(Action<ITypeHandlerConfigurator<TSerializerSource, TDeserializerSource>> configurator)
        {
            configurator.ValidateArgument(nameof(configurator));
            var handler = new TypeHandler();
            configurator(handler);
            _typeHandlers.Add(handler);
        }

        /// <summary>
        /// Serializes <paramref name="instance"/> to an instance of <typeparamref name="TSerializerSource"/>.
        /// </summary>
        /// <param name="instance">The instance with the data to serialize</param>
        /// <returns>The serialized data from <paramref name="instance"/></returns>
        /// <exception cref="NotSupportedException">Thrown when no type handler has been registered that can handle the type of <paramref name="instance"/></exception>
        protected TSerializerSource SerializeFrom(object instance)
        {
            instance.ValidateArgument(nameof(instance));
            var type = instance.GetType();

            var handler = _typeHandlers.FirstOrDefault(x => x.CanSerialize(type));

            return handler != null ? handler.Serialize(type, instance) : throw new NotSupportedException($"No type handler registered that can serialize type <{type}>");
        }

        /// <summary>
        /// Deserializes <paramref name="source"/> to <paramref name="instance"/>.
        /// </summary>
        /// <param name="source">The object with the data to deserialize</param>
        /// <param name="instance">The instance to deserialize the data to</param>
        /// <returns>The object with the deserialized data</returns>
        /// <exception cref="NotSupportedException">Thrown when no type handler has been registered that can handle the type of <paramref name="instance"/></exception>
        protected object DeserializeTo(TDeserializerSource source, object instance)
        {
            source.ValidateArgument(nameof(source));
            instance.ValidateArgument(nameof(instance));

            var type = instance.GetType();

            var handler = _typeHandlers.FirstOrDefault(x => x.CanDeserialize(type));

            return handler != null ? handler.Deserialize(source, instance) : throw new NotSupportedException($"No type handler registered that can deserialize to type <{type}>");
        }

        private class TypeHandler: ITypeHandlerConfigurator<TSerializerSource, TDeserializerSource>
        {
            // Fields
            private List<Predicate<Type>> _conditions = new List<Predicate<Type>>();
            private Func<TDeserializerSource, object, object> _deserializer;
            private Func<Type, object, TSerializerSource> _serializer;

            #region Configuration
            public ITypeHandlerConfigurator<TSerializerSource, TDeserializerSource> Handles(Predicate<Type> predicate)
            {
                predicate.ValidateArgument(nameof(predicate));

                _conditions.Add(predicate);
                return this;
            }

            public ITypeHandlerConfigurator<TSerializerSource, TDeserializerSource> Handles(Type type)
            {
                type.ValidateArgument(nameof(type));

                return Handles(x => x.IsAssignableTo(type));
            }

            public ITypeHandlerConfigurator<TSerializerSource, TDeserializerSource> Handles<TType>()
            {
                return Handles(typeof(TType));
            }

            public ITypeHandlerConfigurator<TSerializerSource, TDeserializerSource> SerializeUsing(Func<Type, object, TSerializerSource> serializer)
            {
                serializer.ValidateArgument(nameof(serializer));

                _serializer = serializer;
                return this;
            }

            public ITypeHandlerConfigurator<TSerializerSource, TDeserializerSource> DeserializeUsing(Func<TDeserializerSource, object, object> deserializer)
            {
                deserializer.ValidateArgument(nameof(deserializer));

                _deserializer = deserializer;
                return this;
            }
            #endregion

            public bool CanSerialize(Type type)
            {
                type.ValidateArgument(nameof(type));

                return _serializer != null && _conditions.Any(x => x(type));
            }

            public bool CanDeserialize(Type type)
            {
                type.ValidateArgument(nameof(type));

                return _deserializer != null && _conditions.Any(x => x(type));
            }

            public TSerializerSource Serialize(Type type, object instance)
            {
                type.ValidateArgument(nameof(type));
                instance.ValidateArgument(nameof(instance));
                _serializer.ValidateArgument(x => x != null, x => new InvalidOperationException($"No serializer delegate set"));

                return _serializer(type, instance);
            }

            public object Deserialize(TDeserializerSource source, object instance)
            {
                source.ValidateArgument(nameof(source));
                instance.ValidateArgument(nameof(instance));
                _deserializer.ValidateArgument(x => x != null, x => new InvalidOperationException($"No deserializer delegate set"));

                return _deserializer(source, instance);
            }
        }
    }

    /// <summary>
    /// Template for creating serializers. Exposes type handlers for serializing/deserializing certain types in a different way.
    /// </summary>
    /// <typeparam name="TSource">The type to serialize to and deserialize from.</typeparam>
    public abstract class BaseSerializer<TSource> : BaseSerializer<TSource, TSource>
    {

    }

    /// <summary>
    /// Template for creating serializers. Exposes type handlers for serializing/deserializing certain types in a different way.
    /// </summary>
    public abstract class BaseSerializer : BaseSerializer<string, string>
    {

    }

    /// <summary>
    /// Exposes configuration when creating a new type handler using <see cref="BaseSerializer{TSerializerSource, TDeserializerSource}"/>.
    /// </summary>
    /// <typeparam name="TSerializerSource">The type to serialize to</typeparam>
    /// <typeparam name="TDeserializerSource">The type to deserialze from</typeparam>
    public interface ITypeHandlerConfigurator<TSerializerSource, TDeserializerSource>
    {
        /// <summary>
        /// Defines that the type handler can handle types passing <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The predicate that checks if the type handler can handle the supplied type</param>
        /// <returns>Current configurator for method chaining</returns>
        ITypeHandlerConfigurator<TSerializerSource, TDeserializerSource> Handles(Predicate<Type> predicate);
        /// <summary>
        /// Defines that the type handler can handle type <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type that can be handled</param>
        /// <returns>Current configurator for method chaining</returns>
        ITypeHandlerConfigurator<TSerializerSource, TDeserializerSource> Handles(Type type);
        /// <summary>
        /// Defines that the type handler can handle type <typeparamref name="TType"/>.
        /// </summary>
        /// <typeparam name="TType">The type that can be handled</typeparam>
        /// <returns>Current configurator for method chaining</returns>
        ITypeHandlerConfigurator<TSerializerSource, TDeserializerSource> Handles<TType>();
        /// <summary>
        /// Defines the delegate that will be used to serialize the supplied object.
        /// </summary>
        /// <param name="serializer">Delegate that will serialize the supplied object. First arg is the object type and the second arg is the object to serialize</param>
        /// <returns>Current configurator for method chaining</returns>
        ITypeHandlerConfigurator<TSerializerSource, TDeserializerSource> SerializeUsing(Func<Type, object, TSerializerSource> serializer);
        /// <summary>
        /// Defines the delegate that will be used to deserialize the supplied <typeparamref name="TDeserializerSource"/> to the supplied object instance.
        /// </summary>
        /// <param name="deserializer">Delegate that will deserialize an instance of <typeparamref name="TDeserializerSource"/> to the supplied object. First arg is the object holding the data to deserialize and the second arg is the instance to deserialize to</param>
        /// <returns>Current configurator for method chaining</returns>
        ITypeHandlerConfigurator<TSerializerSource, TDeserializerSource> DeserializeUsing(Func<TDeserializerSource, object, object> deserializer);
    }
}
