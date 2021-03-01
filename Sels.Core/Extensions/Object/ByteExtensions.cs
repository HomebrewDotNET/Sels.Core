using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Sels.Core.Extensions
{
    public static class ByteExtensions
    {
        public static string ToBase64String(this byte[] item)
        {
            return Convert.ToBase64String(item);
        }

        public static string ToString<TEncoding>(this byte[] item) where TEncoding : Encoding, new()
        {
            var encoding = new TEncoding();

            return encoding.GetString(item);
        }

        #region GetBytes
        public static byte[] GetBytes(this object sourceObject)
        {
            sourceObject.ValidateVariable(nameof(sourceObject));

            BinaryFormatter formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, sourceObject);
                return stream.ToArray();
            }
        }

        public static byte[] GetBytes(this string sourceString)
        {
            return sourceString.GetBytes<UTF8Encoding>();
        }

        public static byte[] GetBytesFromBase64(this string sourceString)
        {
            return Convert.FromBase64String(sourceString);
        }

        public static byte[] GetBytes<TEncoding>(this string sourceString) where TEncoding : Encoding, new()
        {
            var encoding = new TEncoding();
            return encoding.GetBytes(sourceString);
        }

        public static byte[] GetBytes(this double source)
        {
            return BitConverter.GetBytes(source);
        }

        public static byte[] GetBytes(this bool source)
        {
            return BitConverter.GetBytes(source);
        }

        public static byte[] GetBytes(this char source)
        {
            return BitConverter.GetBytes(source);
        }
        #endregion

    }
}
