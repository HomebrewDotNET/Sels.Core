using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Bootstrap.V5.Utilities.Flex
{
    /// <summary>
    /// Contains the flex container types.
    /// </summary>
    public enum FlexContainerType
    {
        /// <summary>
        /// The default container that stretches.
        /// </summary>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.Flex)]
        Default,
        /// <summary>
        /// The container that grows based on the flex items.
        /// </summary>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.InlineFlex)]
        Inline
    }

    /// <summary>
    /// Static extension methods for <see cref="FlexContainerType"/>.
    /// </summary>
    public static class FlexContainerTypeExtensions
    {
        /// <summary>
        /// Gets the css class for <paramref name="flexContainerType"/>.
        /// </summary>
        /// <param name="flexContainerType">Flex container to get the css class for</param>
        /// <returns>The css class for <paramref name="flexContainerType"/></returns>
        public static string ToCss(this FlexContainerType flexContainerType)
        {
            return flexContainerType.GetStringValue();
        }
    }
}
