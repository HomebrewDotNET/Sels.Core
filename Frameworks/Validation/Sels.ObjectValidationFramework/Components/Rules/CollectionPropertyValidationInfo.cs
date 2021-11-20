using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Contracts.Rules;

namespace Sels.ObjectValidationFramework.Components.Rules
{
    /// <summary>
    /// Validation info for validation rules created for a property that is a collection.
    /// </summary>
    public class CollectionPropertyValidationInfo : PropertyValidationInfo
    {
        /// <summary>
        /// Current index of the <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> that is being validated.
        /// </summary>
        public int ValueIndex { get; internal set; }

        /// <summary>
        /// Validation info for validation rules created for a property that is a collection.
        /// </summary>
        internal CollectionPropertyValidationInfo(PropertyInfo property) : base(property)
        {
           
        }
    }
}
