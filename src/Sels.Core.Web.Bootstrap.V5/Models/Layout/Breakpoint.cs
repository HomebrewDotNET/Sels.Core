using Sels.Core.Attributes.Enumeration.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Bootstrap.V5.Layout
{
    /// <summary>
    /// Contains the bootstrap breakpoints.
    /// </summary>
    public enum Breakpoint
    {
        /// <summary>
        /// No breakpoint selected.
        /// </summary>
        [StringEnumValue("")]
        None,
        /// <inheritdoc cref="Bootstrap.Layout.Breakpoints.Small"/>
        [StringEnumValue(Bootstrap.Layout.Breakpoints.Small)]
        Small,
        /// <inheritdoc cref="Bootstrap.Layout.Breakpoints.Medium"/>
        [StringEnumValue(Bootstrap.Layout.Breakpoints.Medium)]
        Medium,
        /// <inheritdoc cref="Bootstrap.Layout.Breakpoints.Large"/>
        [StringEnumValue(Bootstrap.Layout.Breakpoints.Large)]
        Large,
        /// <inheritdoc cref="Bootstrap.Layout.Breakpoints.ExtraLarge"/>
        [StringEnumValue(Bootstrap.Layout.Breakpoints.ExtraLarge)]
        ExtraLarge,
        /// <inheritdoc cref="Bootstrap.Layout.Breakpoints.ExtraExtraLarge"/>
        [StringEnumValue(Bootstrap.Layout.Breakpoints.ExtraExtraLarge)]
        ExtraExtraLarge,
        /// <inheritdoc cref="Bootstrap.Layout.Breakpoints.Full"/>
        [StringEnumValue(Bootstrap.Layout.Breakpoints.Full)]
        Full,
    }
    /// <summary>
    /// Contains extension methods for <see cref="Breakpoint"/>
    /// </summary>
    public static class BreakpointExensions
    {
        /// <summary>
        /// Converts <paramref name="breakpoint"/> to it's bootstrap css equivalent.
        /// </summary>
        /// <param name="breakpoint">The enum to convert</param>
        /// <returns>The css name for <paramref name="breakpoint"/></returns>
        public static string ToCss(this Breakpoint breakpoint)
        {
            return breakpoint.GetStringValue();
        }
    }
}
