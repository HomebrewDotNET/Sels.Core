using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Bootstrap.V5.Layout
{
    /// <summary>
    /// Contains the bootstrap container sizes.
    /// </summary>
    public enum Container
    {
        /// <summary>
        /// No container selected.
        /// </summary>
        [StringEnumValue("")]
        None,
        /// <summary>
        /// The container using the default breakpoint.
        /// </summary>
        [StringEnumValue(Bootstrap.Layout.Container)]
        Default,
        /// <summary>
        /// The container using the <see cref="Bootstrap.Layout.Breakpoints.Small"/> breakpoint.
        /// </summary>
        [StringEnumValue(Bootstrap.Layout.Container + "-" + Bootstrap.Layout.Breakpoints.Small)]
        Small,
        /// <summary>
        /// The container using the <see cref="Bootstrap.Layout.Breakpoints.Medium"/> breakpoint.
        /// </summary>
        [StringEnumValue(Bootstrap.Layout.Container + "-" + Bootstrap.Layout.Breakpoints.Medium)]
        Medium,
        /// <summary>
        /// The container using the <see cref="Bootstrap.Layout.Breakpoints.Large"/> breakpoint.
        /// </summary>
        [StringEnumValue(Bootstrap.Layout.Container + "-" + Bootstrap.Layout.Breakpoints.Large)]
        Large,
        /// <summary>
        /// The container using the <see cref="Bootstrap.Layout.Breakpoints.ExtraLarge"/> breakpoint.
        /// </summary>
        [StringEnumValue(Bootstrap.Layout.Container + "-" + Bootstrap.Layout.Breakpoints.ExtraLarge)]
        ExtraLarge,
        /// <summary>
        /// The container using the <see cref="Bootstrap.Layout.Breakpoints.ExtraExtraLarge"/> breakpoint.
        /// </summary>
        [StringEnumValue(Bootstrap.Layout.Container + "-" + Bootstrap.Layout.Breakpoints.ExtraExtraLarge)]
        ExtraExtraLarge,
        /// <summary>
        /// The container using the <see cref="Bootstrap.Layout.Breakpoints.Full"/> breakpoint.
        /// </summary>
        [StringEnumValue(Bootstrap.Layout.Container + "-" + Bootstrap.Layout.Breakpoints.Full)]
        Full,
    }
    /// <summary>
    /// Contains extension methods for <see cref="Container"/>.
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        /// Converts <paramref name="container"/> to it's css equivalent.
        /// </summary>
        /// <param name="container">The enum to convert</param>
        /// <returns>The css class for <paramref name="container"/></returns>
        public static string ToCss(this Container container)
        {
            return container.GetStringValue();
        }

        /// <summary>
        /// Converts <paramref name="breakpoint"/> to a it's css <see cref="Container"/> equivalent.
        /// </summary>
        /// <param name="breakpoint">The size of the container</param>
        /// <returns>The css for the container of size <paramref name="breakpoint"/></returns>
        public static string ToContainerCss(this Breakpoint breakpoint)
        {
            if (breakpoint == Breakpoint.None) return Container.Default.ToCss();
            return $"{Bootstrap.Layout.Container}-{breakpoint.ToCss()}";
        }
    }
}
