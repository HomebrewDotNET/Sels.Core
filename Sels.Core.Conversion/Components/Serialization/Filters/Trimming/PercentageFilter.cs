using Sels.Core.Conversion.Templates.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Conversion.Serialization.Filters.Conversion
{
    /// <summary>
    /// Filter that removes % at the end of a string on read and adds them on write.
    /// </summary>
    public class PercentageFilter : BaseEndTrimFilter
    {
        /// <summary>
        /// Filter that removes % at the end of a string on read and adds them on write.
        /// </summary>
        public PercentageFilter() : base('%')
        {

        }
    }
}
