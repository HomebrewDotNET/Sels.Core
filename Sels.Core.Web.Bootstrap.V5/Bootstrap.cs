using Microsoft.Extensions.Primitives;

namespace Sels.Core.Web.Bootstrap
{
    /// <summary>
    /// Contains constants related to bootstrap.
    /// </summary>
    public static class Bootstrap
    {
        /// <summary>
        /// Contains the color names re-used by bootstrap components.
        /// </summary>
        public static class Color
        {
            /// <summary>
            /// The primary color.
            /// </summary>
            public const string Primary = "primary";
            /// <summary>
            /// The secondary color.
            /// </summary>
            public const string Secondary = "secondary";
            /// <summary>
            /// The success color.
            /// </summary>
            public const string Success = "success";
            /// <summary>
            /// The primary color.
            /// </summary>
            public const string Danger = "danger";
            /// <summary>
            /// The warning color.
            /// </summary>
            public const string Warning = "warning";
            /// <summary>
            /// The info color.
            /// </summary>
            public const string Info = "info";
            /// <summary>
            /// The light color.
            /// </summary>
            public const string Light = "light";
            /// <summary>
            /// The dark color.
            /// </summary>
            public const string Dark = "dark";
        }

        /// <summary>
        /// Contains constants related to bootstrap layout components.
        /// </summary>
        public static class Layout
        {
            /// <summary>
            /// The name for bootstrap containers.
            /// </summary>
            public const string Container = "container";

            /// <summary>
            /// Contains the bootstrap responsive breakpoints.
            /// </summary>
            public static class Breakpoints
            {
                /// <summary>
                /// The small breakpoint at >= 576px.
                /// </summary>
                public const string Small = "sm";
                /// <summary>
                /// The medium breakpoint at >= 768px.
                /// </summary>
                public const string Medium = "md";
                /// <summary>
                /// The large breakpoint at >= 992px.
                /// </summary>
                public const string Large = "lg";
                /// <summary>
                /// The extra large breakpoint at >= 1200px.
                /// </summary>
                public const string ExtraLarge = "xl";
                /// <summary>
                /// The extra extra large breakpoint at >= 1400px.
                /// </summary>
                public const string ExtraExtraLarge = "xxl";
                /// <summary>
                /// The full width size. 
                /// </summary>
                public const string Full = "fluid";
            }
        }

        /// <summary>
        /// Contains constants related to bootstrap components.
        /// </summary>
        public static class Components
        {
            /// <summary>
            /// The css class for a button.
            /// </summary>
            public const string Button = "btn";
            /// <summary>
            /// The css class for a outline button color.
            /// </summary>
            public const string ButtonOutline = "btn-outline";

            /// <summary>
            /// Contains constants related to bootsrap spinners.
            /// </summary>
            public static class Spinners
            {
                /// <summary>
                /// The css class for a border spinner.
                /// </summary>
                public const string Border = "spinner-border";
                /// <summary>
                /// The css class for a grow spinner.
                /// </summary>
                public const string Grow = "spinner-grow";
            }
        }

        /// <summary>
        /// Contains constants related to bootstrap helpers.
        /// </summary>
        public static class Helpers
        {
            /// <summary>
            /// The css class for a link.
            /// </summary>
            public const string Link = "link";
        }

        /// <summary>
        /// Contains constants related to bootstrap utilities.
        /// </summary>
        public static class Utilities
        {
            /// <summary>
            /// The css class for a text utility.
            /// </summary>
            public const string Text = "text";

            /// <summary>
            /// Contains constants related to bootstrap flexbox utilities.
            /// </summary>
            public static class FlexBox
            {
                /// <summary>
                /// The css class for defining a flexbox container.
                /// </summary>
                public const string Flex = "d-flex";
                /// <summary>
                /// The css class for defining a flexbox container.
                /// </summary>
                public const string InlineFlex = "d-inline-flex";

                /// <summary>
                /// Contains the css directions in a flexbox.
                /// </summary>
                public static class Directions
                {
                    /// <summary>
                    /// The direction css for horizontal direction.
                    /// </summary>
                    public const string Row = "flex-row";
                    /// <summary>
                    /// The direction css for horizontal direction that starts on the opposite side.
                    /// </summary>
                    public const string ReverseRow = "flex-row-reverse";
                    /// <summary>
                    /// The direction css for vertical direction.
                    /// </summary>
                    public const string Column = "flex-column";
                    /// <summary>
                    /// The direction css for vertical direction that starts on the opposite side.
                    /// </summary>
                    public const string ReverseColumn = "flex-column-reverse";
                }

                /// <summary>
                /// Contains the css for the alignment of flex items on the main and cross axis.
                /// </summary>
                public static class Alignments
                {
                    /// <summary>
                    /// Align all items at the start of the container.
                    /// </summary>
                    public const string JustifyStart = "justify-content-start";
                    /// <summary>
                    /// Align all items at the end of the container.
                    /// </summary>
                    public const string JustifyEnd = "justify-content-end";
                    /// <summary>
                    /// Align all items at the center of the container.
                    /// </summary>
                    public const string JustifyCenter = "justify-content-center";
                    /// <summary>
                    /// Fill empty space between the flex items.
                    /// </summary>
                    public const string JustifyBetween = "justify-content-between";
                    /// <summary>
                    /// Fill empty space around the flex items.
                    /// </summary>
                    public const string JustifyAround = "justify-content-around";
                    /// <summary>
                    /// Fill empty space evenly between the flex items.
                    /// </summary>
                    public const string JustifyEvenly = "justify-content-evenly";

                    /// <summary>
                    /// Align items at the start of the cross axis of the container.
                    /// </summary>
                    public const string AlignStart = "align-items-start";
                    /// <summary>
                    /// Align items at the end of the cross axis of the container.
                    /// </summary>
                    public const string AlignEnd = "align-items-end";
                    /// <summary>
                    /// Align items at the center of the cross axis of the container.
                    /// </summary>
                    public const string AlignCenter = "align-items-center";
                    /// <summary>
                    /// Align flex items along their baseline.
                    /// </summary>
                    public const string AlignBaseline = "align-items-baseline";
                    /// <summary>
                    /// Stretch items along the cross axis of the container.
                    /// </summary>
                    public const string AlignStretch = "align-items-stretch";
                }
            }
        }
    }
}
