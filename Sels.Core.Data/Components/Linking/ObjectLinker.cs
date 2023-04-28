using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sels.Core.Data.Linking
{
    /// <inheritdoc cref="IRelationalLinker"/>
    public class ObjectLinker : IRelationalLinker
    {
        // Fields
        private readonly Dictionary<Type, List<object>> _linked = new Dictionary<Type, List<object>>();
        private readonly List<LinkProfile> _profiles = new List<LinkProfile>();

        /// <inheritdoc/>
        public IEnumerable<T> GetAll<T>()
        {
            return _linked.ContainsKey(typeof(T)) ? _linked[typeof(T)].Cast<T>() : Array.Empty<T>();
        }
        /// <inheritdoc/>
        public IEnumerable<object> GetAll(Type type)
        {
            return _linked.ContainsKey(type) ? _linked[type] : new List<object>();
        }

        /// <inheritdoc/>
        public T Link<T>(T firstObject, object secondObject, params object[] additionalObjects)
        {
            var objectsToLink = Helper.Collection.EnumerateAll<object>(firstObject.CastToOrDefault<object>().AsEnumerable(), secondObject.AsEnumerable(), additionalObjects)
                               .Where(x => x != null)
                               .Select(x =>
                               {
                                   var type = x.GetType();
                                   var profile = GetProfile(type);
                                   var id = profile.GetId(x);
                                   if (id == null) return default;

                                   var linkedInstance = GetAll(type).FirstOrDefault(l => profile.GetId(l).Equals(id));

                                   if (linkedInstance != null)
                                   {
                                       return (WasLinked: true, Profile: profile, Instance: linkedInstance);
                                   }
                                   else
                                   {
                                       profile.Initialize(x);
                                       return (WasLinked: false, Profile: profile, Instance: x);
                                   }
                               })
                               .Where(x => x != default);

            foreach(var objectToLink in objectsToLink.Where(x => !x.WasLinked))
            {
                var others = objectsToLink.Where(x => x.Instance != objectToLink.Instance);

                foreach(var other in others)
                {
                    if(objectToLink.Profile.CanLink(objectToLink.Instance, other.Instance))
                    {
                        objectToLink.Profile.Link(objectToLink.Instance, other.Instance);
                    }
                }

                _linked.AddValueToList(objectToLink.Instance.GetType(), objectToLink.GetType());
            }

            return firstObject;
        }
        /// <inheritdoc/>
        public IRelationalLinkBuilder<T> With<T>(Func<T, object> idGetter)
        {
            idGetter.ValidateArgument(nameof(idGetter));

            var profile = new LinkProfile<T>(this, idGetter);
            _profiles.Add(profile);
            return profile;
        }

        private LinkProfile GetProfile(Type type)
        {
            type.ValidateArgument(nameof(type));

            return _profiles.FirstOrDefault(x => x.Type == type) ?? CreateImpliticProfile(type);
        }

        private LinkProfile CreateImpliticProfile(Type type)
        {
            var idProperty = type.GetProperty("Id");

            if (idProperty == null) idProperty = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).First();

            var profile = new LinkProfile(type, x => idProperty.GetValue(x));
            _profiles.Add(profile);
            return profile;
        }

        #region Classes
        private class LinkProfile
        {
            // Fields
            private readonly Func<object, object> _idGetter;
            protected readonly Dictionary<Type, (Func<object, object, bool> LinkCondition, Action<object, object> Linker)> _links = new Dictionary<Type, (Func<object, object, bool> LinkCondition, Action<object, object> Linker)>();
            protected Action<object> _initializer;

            // Properties
            public Type Type { get; }

            public LinkProfile(Type type, Func<object, object> idGetter)
            {
                Type = type.ValidateArgument(nameof(type));
                this._idGetter = idGetter.ValidateArgument(nameof(idGetter));
            }

            public object GetId(object obj)
            {
                obj.ValidateArgument(nameof(obj));

                return _idGetter(obj) ?? throw new InvalidOperationException($"Id returned null for object <{obj}>");
            }

            public void Initialize(object obj)
            {
                obj.ValidateArgument(nameof(obj));
                _initializer?.Invoke(obj);
            }

            public bool CanLink(object source, object target) {
                source.ValidateArgument(nameof(source));
                target.ValidateArgument(nameof(target));
                var targetType = target.GetType();

                return _links.ContainsKey(targetType) && _links[targetType].LinkCondition(source, target);
            }

            public void Link(object source, object target)
            {
                source.ValidateArgument(nameof(source));
                target.ValidateArgument(nameof(target));

                _links[target.GetType()].Linker(source, target);
            }
        }

        private class LinkProfile<T> : LinkProfile, IRelationalLinkBuilder<T>
        {
            // Fields
            private readonly IRelationalLinker _parent;

            public LinkProfile(IRelationalLinker parent, Func<T, object> idGetter) : base(typeof(T), x => idGetter.ValidateArgument(nameof(idGetter))(x.CastTo<T>()))
            {
                _parent = parent.ValidateArgument(nameof(parent));
            }


            public IRelationalLinkBuilder<T> InitializeWith(Action<T> initializer)
            {
                initializer.ValidateArgument(nameof(initializer));

                _initializer = x => initializer(x.CastTo<T>());
                return this;
            }

            public IRelationalLinkBuilder<T> LinkTo<TLink>(Func<T, TLink, bool> condition, Action<T, TLink> linker)
            {
                condition.ValidateArgument(nameof(condition));
                linker.ValidateArgument(nameof(linker));

                _links.AddOrUpdate(typeof(TLink), (new Func<object, object, bool>((s, t) => condition(s.CastTo<T>(), t.CastTo<TLink>())), new Action<object, object>((s, t) => linker(s.CastTo<T>(), t.CastTo<TLink>()))));
                return this;
            }

            public IEnumerable<T1> GetAll<T1>()
            {
                return _parent.GetAll<T1>();
            }

            public T1 Link<T1>(T1 firstObject, object secondObject, params object[] additionalObjects)
            {
                return _parent.Link(firstObject, secondObject, additionalObjects);
            }

            public IRelationalLinkBuilder<T1> With<T1>(Func<T1, object> idGetter)
            {
                return _parent.With<T1>(idGetter);
            }

            public IEnumerable<object> GetAll(Type type)
            {
                return _parent.GetAll(type);
            }
        }
        #endregion
    }
}
