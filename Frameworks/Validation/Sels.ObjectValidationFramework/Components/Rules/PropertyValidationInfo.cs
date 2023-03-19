using Sels.Core.Extensions;
using System.Reflection;

namespace Sels.ObjectValidationFramework.Rules
{
    /// <summary>
    /// Validation info for validation rules created for a property.
    /// </summary>
    public class PropertyValidationInfo 
    {
        /// <summary>
        /// The property that is being validated.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Validation info for validation rules created for a property.
        /// </summary>
        internal PropertyValidationInfo(PropertyInfo property)
        {
            Property = property.ValidateArgument(nameof(property));
        }
    }
}
