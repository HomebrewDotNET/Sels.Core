using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using Sels.ObjectValidationFramework.Components.Validators;
using Sels.ObjectValidationFramework.Contracts.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.ObjectValidationFramework.Components.Rules
{
    /// <summary>
    /// Provides validation rules with the information it needs to validate a value.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    internal class ValidationRuleContext<TEntity> : IValidationRuleContext<TEntity>
    {
        /// <inheritdoc/>
        public TEntity Source { get; }
        /// <inheritdoc/>
        public int? ElementIndex { get; }
        /// <inheritdoc/>
        public Parent[] Parents { get; }
        /// <inheritdoc/>
        public Parent CurrentParent => Parents.HasValue() ? Parents.Last() : null;
        /// <inheritdoc/>
        public object Context { get; }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="source"><see cref="Source"/></param>
        /// <param name="context"><see cref="Context"/></param>
        /// <param name="elementIndex"><see cref="ElementIndex"/></param>
        /// <param name="parents"><see cref="Parents"/></param>
        internal ValidationRuleContext(TEntity source, object context, int? elementIndex = null, Parent[] parents = null)
        {
            Source = source.ValidateArgument(nameof(source));
            ElementIndex = elementIndex;
            Parents = parents;
            Context = context;
        }
    }

    /// <summary>
    /// Provides validation rules with the information it needs to validate a value.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
    internal class ValidationRuleContext<TEntity, TContext> : ValidationRuleContext<TEntity>, IValidationRuleContext<TEntity, TContext>
    {
        /// <inheritdoc/>
        new public TContext Context { get; }
        /// <inheritdoc/>
        public bool WasContextSupplied { get; }

        internal ValidationRuleContext(TEntity source, object context, int? elementIndex = null, Parent[] parents = null) : base(source, context, elementIndex, parents)
        {
            if (context != null && context is TContext typedContext)
            {
                WasContextSupplied = true;
                Context = typedContext;
            }
        }
    }

    /// <summary>
    /// Provides validation rules with the information it needs to validate a value.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
    /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal class ValidationRuleContext<TEntity, TInfo, TContext, TValue> : ValidationRuleContext<TEntity, TContext>, IValidationRuleContext<TEntity, TInfo, TContext, TValue>
    {
        /// <inheritdoc/>
        public TInfo Info { get; }
        /// <inheritdoc/>
        public TValue Value { get; set; }

        internal ValidationRuleContext(TInfo info, TEntity source, object context, int? elementIndex = null, Parent[] parents = null) : base(source, context, elementIndex, parents)
        {
            Info = info.ValidateArgument(nameof(info));
        }
    }
}
