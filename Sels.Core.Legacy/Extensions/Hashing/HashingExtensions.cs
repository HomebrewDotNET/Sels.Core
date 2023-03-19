using System;
using System.Linq;
using System.Security.Cryptography;

namespace Sels.Core.Extensions.Hashing
{
    /// <summary>
    /// Contains extension methods for generating hashes.
    /// </summary>
    public static class HashingExtensions
    {
        #region GenerateHash
        /// <summary>
        /// Generates a hash from the bytes of <paramref name="sourceObject"/>.
        /// </summary>
        /// <typeparam name="THash">The hashing algorithm to use</typeparam>
        /// <param name="sourceObject">The object to generate the hash for</param>
        /// <returns>The hash string for <paramref name="sourceObject"/></returns>
        public static string GenerateHash<THash>(this object sourceObject) where THash : HashAlgorithm
        {
            sourceObject.ValidateArgument(nameof(sourceObject));
            var hashType = typeof(THash);

            using (var hash = HashAlgorithm.Create(hashType.Name))
            {
                var hashedBytes = hash.ComputeHash(sourceObject.GetBytes());

                return hashedBytes.Select(x => x.ToString("x2")).JoinString(string.Empty);
            }
        }
        #endregion
    }
}
