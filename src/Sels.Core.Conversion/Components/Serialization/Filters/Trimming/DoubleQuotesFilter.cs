using Sels.Core.Conversion.Templates.Filters;

namespace Sels.Core.Conversion.Serialization.Filters.Conversion
{
    /// <summary>
    /// Filter that trims double quotes on read and adds them on write.
    /// </summary>
    public class DoubleQuotesFilter : BaseTrimFilter
    {
        /// <summary>
        /// Filter that trims double quotes on read and adds them on write.
        /// </summary>
        public DoubleQuotesFilter() : base('"')
        {

        }
    }
}
