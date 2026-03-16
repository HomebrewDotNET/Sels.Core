using Sels.Core.Extensions;
using System.Reflection;

namespace Sels.ObjectValidationFramework.Validators
{
    /// <summary>
    /// Represents an object in the current hierarchical object structure if property fallthrough is enabled. When fallthrough is executed on a property the object that the property is from will be used as the last parent in the hierachy .
    /// </summary>
    public class Parent
    {
        /// <summary>
        /// The instance of the parent.
        /// </summary>
        public object Instance { get; }
        /// <summary>
        /// The property on <see cref="Instance"/> that the next object in the hierarchy came from. Can be the next parent or the current value that's being validated.
        /// </summary>
        public PropertyInfo ChildProperty { get; }
        /// <summary>
        /// Index of <see cref="Instance"/> if it was part of a collection. Will be null if it wasn't part of a collection.
        /// </summary>
        public int? ElementIndex { get; }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="instance"><see cref="Instance"/></param>
        /// <param name="property"><see cref="ChildProperty"/></param>
        /// <param name="elementIndex"><see cref="ElementIndex"/></param>
        internal Parent(object instance, PropertyInfo property, int? elementIndex = null)
        {
            Instance = instance.ValidateArgument(nameof(instance));
            ChildProperty = property.ValidateArgument(nameof(property));
            ElementIndex = elementIndex;
        }
    }
}
