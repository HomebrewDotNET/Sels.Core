using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Extensions.Object.Byte
{
    public static class ByteExtensions
    {
        public static string ToBase64String(this byte[] item)
        {
            return Convert.ToBase64String(item);
        }
    }
}
