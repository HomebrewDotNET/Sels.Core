using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Caching
{
    public class AlreadyCachedException : Exception
    {
        private const string messageFormat = "{0} was already cached";

        public AlreadyCachedException(object cacheKey) : base(messageFormat.FormatString(cacheKey.ToString()))
        {

        }
    }
}
