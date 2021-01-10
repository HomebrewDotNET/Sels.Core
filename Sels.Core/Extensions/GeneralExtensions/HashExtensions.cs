using Sels.Core.Extensions.General.Generic;
using Sels.Core.Extensions.General.Validation;
using Sels.Core.Extensions.Object.Byte;
using Sels.Core.Extensions.Object.String;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Sels.Core.Extensions.GeneralExtensions
{
    public static class HashExtensions
    {
        public static string GenerateHash<THash>(object sourceObject) where THash : HashAlgorithm
        {
            sourceObject.ValidateVariable(nameof(sourceObject));
            var hashType = typeof(THash);
            hashType.ValidateVariable(x => !x.Equals(typeof(HashAlgorithm)), () => $"Please use an implementation of {typeof(HashAlgorithm)}");

            using(var hash = HashAlgorithm.Create(hashType.Name))
            {
                var hashedBytes = hash.ComputeHash(sourceObject.GetBytes());

                return hashedBytes.Select(x => x.ToString("x2")).JoinString(string.Empty);
            }
        }
    }
}
