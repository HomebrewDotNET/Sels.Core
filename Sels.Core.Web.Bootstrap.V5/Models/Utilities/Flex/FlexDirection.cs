using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Bootstrap.V5.Utilities.Flex
{
    /// <summary>
    /// Contains the flexbox directions
    /// </summary>
    public enum FlexDirection
    {
        /// <summary>
        /// The browser default.
        /// </summary>
        [StringEnumValue("")]
        Default,
        /// <inheritdoc cref="Bootstrap.Utilities.FlexBox.Directions.Row"/>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.Directions.Row)]
        Row,
        /// <inheritdoc cref="Bootstrap.Utilities.FlexBox.Directions.ReverseRow"/>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.Directions.ReverseRow)]
        ReverseRow,
        /// <inheritdoc cref="Bootstrap.Utilities.FlexBox.Directions.Column"/>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.Directions.Column)]
        Column,
        /// <inheritdoc cref="Bootstrap.Utilities.FlexBox.Directions.ReverseColumn"/>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.Directions.ReverseColumn)]
        ReverseColumn
    }

    /// <summary>
    /// Static extension methods for <see cref="FlexDirection"/>.
    /// </summary>
    public static class FlexDirectionExtensions
    {
        /// <summary>
        /// Gets the css class for <paramref name="flexDirection"/>.
        /// </summary>
        /// <param name="flexDirection">Flex direction to get the css class for</param>
        /// <returns>The css class for <paramref name="flexDirection"/></returns>
        public static string ToCss(this FlexDirection flexDirection)
        {
            return flexDirection.GetStringValue();
        }
    }
}
