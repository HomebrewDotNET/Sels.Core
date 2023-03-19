using System;
using System.Threading.Tasks;

namespace Sels.Core
{
    /// <summary>
    /// Contains delegate definitions.
    /// </summary>
    public static class Delegates
    {
        #region Comparator
        /// <summary>
        /// Encapsulates a method that compares to objects of type <typeparamref name="T"/> to see if they are equal, matching, ...
        /// </summary>
        /// <typeparam name="T">Type of objects to compare</typeparam>
        /// <param name="arg1">Object to compare</param>
        /// <param name="arg2">Object to compare</param>
        /// <returns>Boolean indicating if arg1 is equal, matching, ... to arg2</returns>
        public delegate bool Comparator<in T>(T arg1, T arg2);
        #endregion

        #region Condition
        /// <summary>
        /// Encapsulates a method that defines a condition that is checked when calling this delegate.
        /// </summary>
        /// <returns>Whether or not this condition passes</returns>
        public delegate bool Condition();

        /// <summary>
        /// Encapsulates a method that defines a condition that is checked using the provided arguments when calling this delegate.
        /// </summary>
        /// <typeparam name="T">Condition argument</typeparam>
        /// <returns>Whether or not this condition passes</returns>
        public delegate bool Condition<in T>(T arg);

        /// <summary>
        /// Encapsulates a method that defines a condition that is checked using the provided arguments when calling this delegate.
        /// </summary>
        /// <typeparam name="T1">Condition argument</typeparam>
        /// <typeparam name="T2">Condition argument</typeparam>
        /// <returns>Whether or not this condition passes</returns>
        public delegate bool Condition<in T1, in T2>(T1 arg1, T2 arg2);

        /// <summary>
        /// Encapsulates a method that defines a condition that is checked using the provided arguments when calling this delegate.
        /// </summary>
        /// <typeparam name="T1">Condition argument</typeparam>
        /// <typeparam name="T2">Condition argument</typeparam>
        /// <typeparam name="T3">Condition argument</typeparam>
        /// <returns>Whether or not this condition passes</returns>
        public delegate bool Condition<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);

        /// <summary>
        /// Encapsulates a method that defines a condition that is checked using the provided arguments when calling this delegate.
        /// </summary>
        /// <typeparam name="T1">Condition argument</typeparam>
        /// <typeparam name="T2">Condition argument</typeparam>
        /// <typeparam name="T3">Condition argument</typeparam>
        /// <typeparam name="T4">Condition argument</typeparam>
        /// <returns>Whether or not this condition passes</returns>
        public delegate bool Condition<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

        /// <summary>
        /// Encapsulates a method that defines a condition that is checked using the provided arguments when calling this delegate.
        /// </summary>
        /// <typeparam name="T1">Condition argument</typeparam>
        /// <typeparam name="T2">Condition argument</typeparam>
        /// <typeparam name="T3">Condition argument</typeparam>
        /// <typeparam name="T4">Condition argument</typeparam>
        /// <typeparam name="T5">Condition argument</typeparam>
        /// <returns>Whether or not this condition passes</returns>
        public delegate bool Condition<in T1, in T2, in T3, in T4, in T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
        #endregion

        /// <summary>
        /// Contains async version of existing delegates
        /// </summary>
        public static class Async
        {
            #region Predicate
            /// <inheritdoc cref="Predicate{T}"/>
            public delegate Task<bool> AsyncPredicate<in T>(T arg);
            #endregion

            #region Condition
            /// <inheritdoc cref="Condition"/>
            public delegate Task<bool> AsyncCondition();

            /// <inheritdoc cref="Condition{T}"/>
            public delegate Task<bool> AsyncCondition<in T>(T arg);

            /// <inheritdoc cref="Condition{T1, T2}"/>
            public delegate Task<bool> AsyncCondition<in T1, in T2>(T1 arg1, T2 arg2);

            /// <inheritdoc cref="Condition{T1, T2, T3}"/>
            public delegate Task<bool> AsyncCondition<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);

            /// <inheritdoc cref="Condition{T1, T2, T3, T4}"/>
            public delegate Task<bool> AsyncCondition<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

            /// <inheritdoc cref="Condition{T1, T2, T3, T4, T5}"/>
            public delegate Task<bool> AsyncCondition<in T1, in T2, in T3, in T4, in T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
            #endregion

            #region Action
            /// <inheritdoc cref="Action"/>
            public delegate Task AsyncAction();

            /// <inheritdoc cref="Action{T}"/>
            public delegate Task AsyncAction<in T>(T arg);

            /// <inheritdoc cref="Action{T1, T2}"/>
            public delegate Task AsyncAction<in T1, in T2>(T1 arg1, T2 arg2);

            /// <inheritdoc cref="Action{T1, T2, T3}"/>
            public delegate Task AsyncAction<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);

            /// <inheritdoc cref="Action{T1, T2, T3, T4}"/>
            public delegate Task AsyncAction<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

            /// <inheritdoc cref="Action{T1, T2, T3, T4, T5}"/>
            public delegate Task AsyncAction<in T1, in T2, in T3, in T4, in T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
            #endregion

            #region Function
            /// <inheritdoc cref="Func{TResult}"/>
            public delegate Task<TResult> AsyncFunc<TResult>();

            /// <inheritdoc cref="Func{T, TResult}"/>
            public delegate Task<TResult> AsyncFunc<in T, TResult>(T arg);

            /// <inheritdoc cref="Func{T1, T2, TResult}"/>
            public delegate Task<TResult> AsyncFunc<in T1, in T2, TResult>(T1 arg1, T2 arg2);

            /// <inheritdoc cref="Func{T1, T2, T3, TResult}"/>
            public delegate Task<TResult> AsyncFunc<in T1, in T2, in T3, TResult>(T1 arg1, T2 arg2, T3 arg3);

            /// <inheritdoc cref="Func{T1, T2, T3, T4, TResult}"/>
            public delegate Task<TResult> AsyncFunc<in T1, in T2, in T3, in T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

            /// <inheritdoc cref="Func{T1, T2, T3, T4, T5, TResult}"/>
            public delegate Task<TResult> AsyncFunc<in T1, in T2, in T3, in T4, in T5, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
            #endregion
        }
    }
}
