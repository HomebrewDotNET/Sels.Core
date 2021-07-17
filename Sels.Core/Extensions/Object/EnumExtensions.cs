using Sels.Core.Components.Enumeration.Value;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Extensions.Object
{
    public static class EnumExtensions
    {
        #region GetValue
        /// <summary>
        /// Gets the value of the <see cref="EnumValue"/> attribute if it's defined. Otherwise return enumeration as string.
        /// </summary>
        public static string GetValue(this Enum enumeration)
        {
            enumeration.ValidateVariable(nameof(enumeration));

            var attribute = enumeration.GetAttributeOrDefault<EnumValue>();

            if (attribute.HasValue())
            {
                return attribute.Value;
            }

            return enumeration.ToString();
        }
        #endregion
    }
}
