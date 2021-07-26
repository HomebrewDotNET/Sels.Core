using Sels.Core.Contracts.Factory;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Factory
{
    /// <summary>
    /// Factory that creates new instances by calling the right constructor based on supplied arguments.
    /// </summary>
    /// <typeparam name="T">Type of new instances that factory can create</typeparam>
    public class GenericFactory<T> : IFactory<T>
    {
        /// <summary>
        /// Type of new instances that factory can create.
        /// </summary>
        public Type InstanceType => typeof(T);

        public T Create(params object[] arguments)
        {
            return CreateNewInstance<T>(arguments);
        }

        protected virtual TInstance CreateNewInstance<TInstance>(params object[] arguments) where TInstance : T
        {
            typeof(TInstance).ValidateArgumentCanBeContructedWithArguments(nameof(arguments), arguments);

            return typeof(TInstance).Construct<TInstance>(arguments);
        }
    }
}
