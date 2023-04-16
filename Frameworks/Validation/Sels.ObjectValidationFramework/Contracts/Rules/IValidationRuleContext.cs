﻿using Sels.ObjectValidationFramework.Validators;

namespace Sels.ObjectValidationFramework.Rules
{
    /// <summary>
    /// Provides validation rules with the information it needs to validate a value.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TContext">Tyope of the optional context that can be supplied to a validation profile</typeparam>
    public interface IValidationRuleContext<out TEntity, out TContext>
    {
        /// <summary>
        /// The entity that is being validated.
        /// </summary>
        TEntity Source { get; }
        /// <summary>
        /// Index of <see cref="Source"/> if it was part of a collection. Will be null if it wasn't part of a collection.
        /// </summary>
        int? ElementIndex { get; }
        /// <summary>
        /// Hierarchy of object if property fallthrough is enabled. The previous element is always the parent of the next element.
        /// </summary>
        Parent[] Parents { get; }
        /// <summary>
        /// The parent of the value that's being validated. Is null when property fallthough is disabled or when <see cref="Source"/> is the first object in the hierarchy.
        /// </summary>
        Parent CurrentParent { get; }
        /// <summary>
        /// Optional context that can be supplied to a validation rule.
        /// </summary>
        TContext Context { get; }
        /// <summary>
        /// If a context of type <typeparamref name="TContext"/> was supplied to the validation rule. If true then <see cref="Context"/> will be set, otherwise <see cref="Context"/> will the default value of <typeparamref name="TContext"/>.
        /// </summary>
        bool HasContext { get; }
        /// <summary>
        /// Property that can be used to pass data from the validator delegate to the error constructor delegate.
        /// </summary>
        object ValidatorResult { get; set; }
    }

    /// <summary>
    /// Provides validation rules with the information it needs to validate a value.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation target can use depending on what is being validated</typeparam>
    /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
    /// <typeparam name="TValue">Type of the value that is being validated</typeparam>
    public interface IValidationRuleContext<out TEntity, out TInfo, out TContext, out TValue> : IValidationRuleContext<TEntity, TContext>
    {
        /// <summary>
        /// Contains additional information about the current validation target.
        /// </summary>
        TInfo Info { get; }

        /// <summary>
        /// The value that is being validated.
        /// </summary>
        TValue Value { get; }
    }
}
