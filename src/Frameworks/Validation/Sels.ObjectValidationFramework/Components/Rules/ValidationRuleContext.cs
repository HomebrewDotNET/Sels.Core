using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Validators;
using System.Linq;

namespace Sels.ObjectValidationFramework.Rules
{
    /// <summary>
    /// Provides validation rules with the information it needs to validate a value.
    /// </summary>
    /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
    /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
    internal class ValidationRuleContext<TEntity, TContext> : IValidationRuleContext<TEntity, TContext>
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
        public TContext Context { get; }
        /// <inheritdoc/>
        public bool HasContext => Context != null;
        /// <inheritdoc/>
        public object ValidatorResult { get; set; }

        /// <inheritdoc cref="ValidationRuleContext{TEntity, TContext}"/>
        /// <param name="source"><inheritdoc cref="Source"/></param>
        /// <param name="context"><inheritdoc cref="Context"/></param>
        /// <param name="elementIndex"><inheritdoc cref="ElementIndex"/></param>
        /// <param name="parents"><inheritdoc cref="Parents"/></param>
        internal ValidationRuleContext(TEntity source, TContext context, int? elementIndex = null, Parent[] parents = null)
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
    /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
    /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
    /// <typeparam name="TValue">Type of value that is being validated</typeparam>
    internal class ValidationRuleContext<TEntity, TInfo, TContext, TValue> : ValidationRuleContext<TEntity, TContext>, IValidationRuleContext<TEntity, TInfo, TContext, TValue>
    {
        /// <inheritdoc/>
        public TInfo Info { get; }
        /// <inheritdoc/>
        public TValue Value { get; set; }

        internal ValidationRuleContext(TInfo info, TEntity source, TContext context, int? elementIndex = null, Parent[] parents = null) : base(source, context, elementIndex, parents)
        {
            Info = info.ValidateArgument(nameof(info));
        }
    }
}
