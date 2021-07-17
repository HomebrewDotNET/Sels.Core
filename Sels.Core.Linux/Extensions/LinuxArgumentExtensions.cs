using Sels.Core.Linux.Components.LinuxCommand.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions.Reflection;

namespace Sels.Core.Linux.Extensions.Argument
{
    public static class LinuxArgumentExtensions
    {
        /// <summary>
        /// Returns value from <see cref="LinuxValueAttribute"/> if it's defined on <paramref name="propertyValue"/>, otherwise return ToString() of <paramref name="propertyValue"/>.
        /// </summary>
        /// <param name="propertyValue">Object to get value from</param>
        /// <returns>Argument value</returns>
        public static string GetArgumentValue(this object propertyValue)
        {
            var attribute = (LinuxValueAttribute)ReflectionExtensions.GetAttributeOrDefault<LinuxValueAttribute>((dynamic)propertyValue);

            return attribute?.Value?.ToString() ?? propertyValue.ToString();
        }
    }
}
