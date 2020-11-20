using Sels.Core.Extensions.General.Validation;
using System;
using System.Collections.Generic;
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
                return hash.ComputeHash(sourceObject.GetBytes());
            }
        }
    }
}
