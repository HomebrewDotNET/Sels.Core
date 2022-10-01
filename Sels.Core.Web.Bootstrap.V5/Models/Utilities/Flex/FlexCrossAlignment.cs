using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Bootstrap.V5.Utilities.Flex
{
    /// <summary>
    /// The alignment of flex items along the cross axis of the container.
    /// </summary>
    public enum FlexCrossAlignment
    {
        /// <summary>
        /// The browser default.
        /// </summary>
        [StringEnumValue("")]
        Default,
        /// <inheritdoc cref="Bootstrap.Utilities.FlexBox.Alignments.AlignStart"/>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.Alignments.AlignStart)]
        Start,
        /// <inheritdoc cref="Bootstrap.Utilities.FlexBox.Alignments.AlignEnd"/>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.Alignments.AlignEnd)]
        End,
        /// <inheritdoc cref="Bootstrap.Utilities.FlexBox.Alignments.AlignCenter"/>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.Alignments.AlignCenter)]
        Center,
        /// <inheritdoc cref="Bootstrap.Utilities.FlexBox.Alignments.AlignBaseline"/>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.Alignments.AlignBaseline)]
        Baseline,
        /// <inheritdoc cref="Bootstrap.Utilities.FlexBox.Alignments.AlignStretch"/>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.Alignments.AlignStretch)]
        Stretch
    }

    /// <summary>
    /// Static extension methods for <see cref="FlexCrossAlignment"/>.
    /// </summary>
    public static class FlexCrossAlignmentExtensions
    {
        /// <summary>
        /// Gets the css class for <paramref name="alignment"/>.
        /// </summary>
        /// <param name="alignment">Flex alignment to get the css class for</param>
        /// <returns>The css class for <paramref name="alignment"/></returns>
        public static string ToCss(this FlexCrossAlignment alignment)
        {
            return alignment.GetStringValue();
        }
    }
}
