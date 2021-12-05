using Sels.ObjectValidationFramework.Components.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.ObjectValidationFramework.Contracts.Rules
{
    /// <summary>
    /// Provides validation rules with the information it needs to validate a value.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    public interface IValidationRuleContext<TEntity>
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
        object Context { get; }
    }

    /// <summary>
    /// Provides validation rules with the information it needs to validate a value.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
    public interface IValidationRuleContext<TEntity, TContext> : IValidationRuleContext<TEntity>
    {
        /// <summary>
        /// Optional context that can be supplied to a validation rule. Is the default value for type <typeparamref name="TContext"/> if no context was supplied.
        /// </summary>
        new TContext Context { get; }
        /// <summary>
        /// If a context of type <typeparamref name="TContext"/> was supplied to the validation rule. If true then <see cref="Context"/> will be set, otherwise <see cref="Context"/> will the default value of <typeparamref name="TContext"/>.
        /// </summary>
        bool WasContextSupplied { get; }
    }

    /// <summary>
    /// Provides validation rules with the information it needs to validate a value.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
    /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    public interface IValidationRuleContext<TEntity, TInfo, TContext, TValue> : IValidationRuleContext<TEntity, TContext>
    {
        /// <summary>
        /// Contains additional information about the current validation rule.
        /// </summary>
        TInfo Info { get; }

        /// <summary>
        /// The value that is being validated.
        /// </summary>
        TValue Value { get; }
    }
}
