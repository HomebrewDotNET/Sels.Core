using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Blazor.Authentication.Token
{
    /// <summary>
    /// Service that manages tokens used for authentication in Blazor servers apps.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a token using <paramref name="claims"/> that describes the user.
        /// </summary>
        /// <param name="claims">The claims of the user</param>
        /// <param name="expiryDate">Optional date when the token expires</param>
        /// <returns>The generated token.</returns>
        Task<string> CreateTokenAsync(IEnumerable<Claim> claims, DateTime? expiryDate = null);
        /// <summary>
        /// Validates <paramref name="token"/>.
        /// </summary>
        /// <param name="token">The token to validate</param>
        /// <returns>True if <paramref name="token"/> is valid, otherwise false</returns>
        Task<bool> ValidateTokenAsync(string token);
        /// <summary>
        /// Extracts the principal from <paramref name="token"/>.
        /// </summary>
        /// <param name="token">Token to get the principal from</param>
        /// <returns>The principal embedded in <paramref name="token"/> or null if the token isn't valid anymore</returns>
        Task<ClaimsPrincipal?> GetPrincipal(string token);
    }
}
