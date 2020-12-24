using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Configuration.Exceptions
{
    public class NoSectionSuppliedException : Exception
    {
        private const string ErrorMessage = "At least 1 section key must be supplied";

        public NoSectionSuppliedException() : base (ErrorMessage)
        {

        }
    }
}
