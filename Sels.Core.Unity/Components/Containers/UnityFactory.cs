using Sels.Core.Components.Caching;
using Sels.Core.Extensions.Execution.Linq;
using Sels.Core.Extensions.General.Generic;
using Sels.Core.Extensions.General.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;

namespace Sels.Core.Unity.Components.Containers
{
    public static class UnityFactory
    {
        // Fields
        private static readonly object _threadLock = new object();
        private static readonly List<Action<IUnityContainer>> _registerActions = new List<Action<IUnityContainer>>();

        // Properties
        public static ValueCache<IUnityContainer> Container { get; }

        static UnityFactory()
        {
            Container = new ValueCache<IUnityContainer>(_threadLock, () =>
            {
                IUnityContainer container = new UnityContainer();

                _registerActions.Execute(x => x(container));

                return container;
            });
        }

        #region Setup
        public static void Register(Action<IUnityContainer> registerAction)
        {
            registerAction.ValidateVariable(nameof(registerAction));

            lock (_threadLock)
            {
                _registerActions.Add(registerAction);
            }
        }
        public static void Register(params Action<IUnityContainer>[] registerActions)
        {
            registerActions.Execute(x => Register(x));
        }

        public static void RegisterAndRebuildContainer(Action<IUnityContainer> registerAction)
        {
            registerAction.ValidateVariable(nameof(registerAction));

            lock (_threadLock)
            {
                _registerActions.Add(registerAction);
                ClearAndRebuildContainer();
            }
        }
        public static void RegisterAndRebuildContainer(params Action<IUnityContainer>[] registerActions)
        {
            registerActions.Execute(x => Register(x));

            lock (_threadLock)
            {
                ClearAndRebuildContainer();
            }
        }

        public static void ClearContainer()
        {
            Container.ResetCache();
        }

        public static void ClearAndRebuildContainer()
        {
            ClearContainer();
            _ = Container.Value;
        }

        public static void ClearRegisterActions()
        {
            lock (_threadLock)
            {
                _registerActions.RemoveAll(x => true);
            }
        }
        #endregion

        #region Resolve
        public static T Resolve<T>()
        {
            return Container.Value.Resolve<T>();
        }

        public static T Resolve<T>(string name)
        {
            name.ValidateVariable(nameof(name));

            return Container.Value.Resolve<T>(name);
        }

        public static IEnumerable<T> ResolveAll<T>()
        {
            return Container.Value.ResolveAll<T>();
        }

        public static object Resolve(Type type)
        {
            type.ValidateVariable(nameof(type));

            return Container.Value.Resolve(type);
        }

        public static object Resolve(Type type, string name)
        {
            name.ValidateVariable(nameof(name));
            type.ValidateVariable(nameof(type));

            return Container.Value.Resolve(type, name);
        }

        public static IEnumerable<object> ResolveAll(Type type)
        {
            type.ValidateVariable(nameof(type));

            return Container.Value.ResolveAll(type);
        }
        #endregion
    }
}
