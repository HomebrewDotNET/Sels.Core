using Sels.Core.Conversion.Templates.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Conversion.Serialization.Filters.Conversion
{
    /// <summary>
    /// Filter that trims single quotes on read and adds them on write.
    /// </summary>
    public class SingleQuotesFilter : BaseTrimFilter
    {
        /// <summary>
        /// Filter that trims single quotes on read and adds them on write.
        /// </summary>
        public SingleQuotesFilter() : base('\'')
        {

        }
    }
}
