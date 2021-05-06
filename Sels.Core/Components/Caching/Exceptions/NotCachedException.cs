using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Caching.Exceptions
{
    public class NotCachedException : Exception
    {
        private const string _messageFormat = "{0} was not cached";

        public NotCachedException(object cacheKey) : base(_messageFormat.FormatString(cacheKey.ToString()))
        {

        }
    }
}
