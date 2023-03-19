using Sels.Core.Attributes.Enumeration.Value;

namespace Sels.Core.Web.Bootstrap.V5.Utilities.Flex
{
    /// <summary>
    /// The alignment of flex items along the main axis of the container.
    /// </summary>
    public enum FlexMainAlignment
    {
        /// <summary>
        /// The browser default.
        /// </summary>
        [StringEnumValue("")]
        Default,
        /// <inheritdoc cref="Bootstrap.Utilities.FlexBox.Alignments.JustifyStart"/>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.Alignments.JustifyStart)]
        Start,
        /// <inheritdoc cref="Bootstrap.Utilities.FlexBox.Alignments.JustifyEnd"/>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.Alignments.JustifyEnd)]
        End,
        /// <inheritdoc cref="Bootstrap.Utilities.FlexBox.Alignments.JustifyCenter"/>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.Alignments.JustifyCenter)]
        Center,
        /// <inheritdoc cref="Bootstrap.Utilities.FlexBox.Alignments.JustifyBetween"/>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.Alignments.JustifyBetween)]
        Between,
        /// <inheritdoc cref="Bootstrap.Utilities.FlexBox.Alignments.JustifyAround"/>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.Alignments.JustifyAround)]
        Around,
        /// <inheritdoc cref="Bootstrap.Utilities.FlexBox.Alignments.JustifyEvenly"/>
        [StringEnumValue(Bootstrap.Utilities.FlexBox.Alignments.JustifyEvenly)]
        Evenly
    }

    /// <summary>
    /// Static extension methods for <see cref="FlexMainAlignment"/>.
    /// </summary>
    public static class FlexMainAlignmentExtensions
    {
        /// <summary>
        /// Gets the css class for <paramref name="alignment"/>.
        /// </summary>
        /// <param name="alignment">Flex alignment to get the css class for</param>
        /// <returns>The css class for <paramref name="alignment"/></returns>
        public static string ToCss(this FlexMainAlignment alignment)
        {
            return alignment.GetStringValue();
        }
    }
}
