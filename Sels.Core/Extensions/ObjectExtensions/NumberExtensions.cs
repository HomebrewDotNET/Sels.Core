using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Extensions.Object.Number
{
    public static class NumberExtensions
    {
        public static uint ToUInt32(this int number)
        {
            return Convert.ToUInt32(number);
        }

        public static int ToInt(this uint number)
        {
            return Convert.ToInt32(number);
        }
    }
}
