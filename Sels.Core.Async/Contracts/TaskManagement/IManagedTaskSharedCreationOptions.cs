﻿using Sels.Core.Extensions;
using Sels.Core.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Async.TaskManagement
{
    /// <summary>
    /// Exposes more options for <see cref="Task"/>(s) scheduled using a <see cref="ITaskManager"/>.
    /// Contains options that can be used for both <see cref="IManagedTask"/> and <see cref="IManagedAnonymousTask"/>.
    /// </summary>
    /// <typeparam name="TInput">The input for the task to create</typeparam>
    /// <typeparam name="TOutput">The output generated by the task</typeparam>
    /// <typeparam name="TDerived">The type of the deriving type. Used for the fluent syntax</typeparam>
    public interface IManagedTaskSharedCreationOptions<TInput, TOutput, TDerived>
    {
        /// <summary>
        /// Defines the <see cref="TaskCreationOptions"/> when scheduling the task on the thread pool.
        /// </summary>
        /// <param name="options">The options for the task to create</param>
        /// <returns>Current options for method chaining</returns>
        TDerived WithCreationOptions(TaskCreationOptions options);
        /// <summary>
        /// Defines the <see cref="ManagedTaskOptions"/> for the created managed task.
        /// </summary>
        /// <param name="options">The options for the task to create</param>
        /// <returns>Current options for method chaining</returns>
        TDerived WithManagedOptions(ManagedTaskOptions options);

        /// <summary>
        /// Defines an action to execute before the managed task.
        /// </summary>
        /// <param name="action">The delegate that will be executed before the managed task</param>
        /// <returns>Current options for method chaining</returns>
        TDerived ExecuteFirst(AsyncAction<TInput, CancellationToken> action);
        /// <summary>
        /// Defines an action to execute after the managed task.
        /// </summary>
        /// <param name="action">The delegate that will be executed before the managed task</param>
        /// <returns>Current options for method chaining</returns>
        TDerived ExecuteAfter(AsyncAction<TInput, TOutput, CancellationToken> action);

        /// <summary>
        /// Sets the properties on the task to create.
        /// </summary>
        /// <param name="action">Delegate that sets the properties</param>
        /// <returns>Current options for method chaining</returns>
        TDerived SetProperties(Action<IDictionary<string, object>> action);    

        #region ExecuteFirst
        /// <summary>
        /// Delays the execution of the managed task by <paramref name="delay"/>.
        /// </summary>
        /// <param name="delay">How much to delay the execution by</param>
        /// <returns>Current options for method chaining</returns>
        TDerived DelayStartBy(TimeSpan delay) => ExecuteFirst((i, t) => Task.Delay(delay));
        /// <summary>
        /// Delays the execution of the managed task by <paramref name="delay"/> ms.
        /// </summary>
        /// <param name="delay">How many milliseconds to delay the execution by</param>
        /// <returns>Current options for method chaining</returns>
        TDerived DelayStartBy(int delay) => ExecuteFirst((i, t) => Task.Delay(delay));
        #endregion

        #region SetProperties
        /// <summary>
        /// Adds a property with name <paramref name="name"/> to the created task.
        /// </summary>
        /// <typeparam name="T">The type of the property to add</typeparam>
        /// <param name="name">The name of the property to add</param>
        /// <param name="value">The value of the property to add</param>
        /// <param name="overwrite">If the property should be overwritten if it already exists</param>
        /// <returns>Current options for method chaining</returns>
        TDerived WithProperty<T>(string name, T value, bool overwrite = false)
        {
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            if(overwrite)
            {
                return SetProperties(x => x.AddOrUpdate(name, value));
            }

            return SetProperties(x => x.Add(name, value));
        }
        /// <summary>
        /// Adds a property with name <paramref name="name"/> to the created task if it doesn't exist already.
        /// </summary>
        /// <typeparam name="T">The type of the property to add</typeparam>
        /// <param name="name">The name of the property to add</param>
        /// <param name="value">The value of the property to add</param>
        /// <returns>Current options for method chaining</returns>
        TDerived WithPropertyIfMissing<T>(string name, T value)
        {
            name.ValidateArgumentNotNullOrWhitespace(nameof(name));

            return SetProperties(x => {
                if (!x.ContainsKey(name)) x.Add(name, value);
            });
        }
        #endregion
    }
}
