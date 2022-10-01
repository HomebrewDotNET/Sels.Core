using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Sels.Core.Extensions
{
    /// <summary>
    /// Contains extension methods for working with <see cref="byte"/>.
    /// </summary>
    public static class ByteExtensions
    {
        /// <summary>
        /// Converts <paramref name="source"/> to a base 64 string.
        /// </summary>
        /// <param name="source">The bytes to convert</param>
        /// <returns>The base 64 string from <paramref name="source"/></returns>
        public static string ToBase64String(this byte[] source)
        {
            source.ValidateArgument(nameof(source));

            return Convert.ToBase64String(source);
        }
        /// <summary>
        /// Converts <paramref name="source"/> to a string decoded using encoding of type <typeparamref name="TEncoding"/>.
        /// </summary>
        /// <typeparam name="TEncoding">The encoding to use</typeparam>
        /// <param name="source">The bytes to convert</param>
        /// <returns>The decoded string from <paramref name="source"/></returns>
        public static string ToString<TEncoding>(this byte[] source) where TEncoding : Encoding, new()
        {
            source.ValidateArgument(nameof(source));
            var encoding = new TEncoding();

            return encoding.GetString(source);
        }

        #region GetBytes
        /// <summary>
        /// Gets the bytes from object <paramref name="sourceObject"/>.
        /// </summary>
        /// <param name="sourceObject">The object to get the bytes from</param>
        /// <returns>The bytes from <paramref name="sourceObject"/></returns>
        [Obsolete]
        public static byte[] GetBytes(this object sourceObject)
        {
            sourceObject.ValidateArgument(nameof(sourceObject));

            BinaryFormatter formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, sourceObject);
                return stream.ToArray();
            }
        }
        /// <summary>
        /// Encodes <paramref name="source"/> using <see cref="UTF8Encoding"/>.
        /// </summary>
        /// <param name="source">The string to encode</param>
        /// <returns>The encoded string as bytes</returns>
        public static byte[] GetBytes(this string source)
        {
            source.ValidateArgument(nameof(source));

            return source.GetBytes<UTF8Encoding>();
        }
        /// <summary>
        /// Encodes <paramref name="source"/> from base 64.
        /// </summary>
        /// <param name="source">The string to encode</param>
        /// <returns>The encoded string as bytes</returns>
        public static byte[] GetBytesFromBase64(this string source)
        {
            source.ValidateArgument(nameof(source));

            return Convert.FromBase64String(source);
        }
        /// <summary>
        /// Encodes <paramref name="source"/> using encoding of type <typeparamref name="TEncoding"/>.
        /// </summary>
        /// <typeparam name="TEncoding">The encoding to use</typeparam>
        /// <param name="source">The string to encode</param>
        /// <returns>The encoded string as bytes</returns>
        public static byte[] GetBytes<TEncoding>(this string source) where TEncoding : Encoding, new()
        {
            source.ValidateArgument(nameof(source));

            var encoding = new TEncoding();
            return encoding.GetBytes(source);
        }
        /// <summary>
        /// Gets the bytes representing <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The double to get the bytes from</param>
        /// <returns>The bytes representing <paramref name="source"/></returns>
        public static byte[] GetBytes(this double source)
        {
            return BitConverter.GetBytes(source);
        }
        /// <summary>
        /// Gets the bytes representing <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The bool to get the bytes from</param>
        /// <returns>The bytes representing <paramref name="source"/></returns>
        public static byte[] GetBytes(this bool source)
        {
            return BitConverter.GetBytes(source);
        }
        /// <summary>
        /// Gets the bytes representing <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The char to get the bytes from</param>
        /// <returns>The bytes representing <paramref name="source"/></returns>
        public static byte[] GetBytes(this char source)
        {
            return BitConverter.GetBytes(source);
        }
        #endregion
    }
}
