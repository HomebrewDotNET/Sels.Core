using System.Security.Claims;

namespace Sels.Core.Web.Blazor.Authentication
{
    /// <summary>
    /// Service that adds ways to set or clear the authentication state for server-side blazor applications.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Sets the authentication state using <paramref name="principal"/>.
        /// </summary>
        /// <param name="principal">The principal to sign in</param>
        /// <param name="expiryDate">Time when the sign in state expires</param>
        /// <returns>Task to await the result</returns>
        Task SignIn(ClaimsPrincipal principal, DateTime? expiryDate = null);
        /// <summary>
        /// Clears any authentication state.
        /// </summary>
        /// <returns>Task to await the result</returns>
        Task SignOut();
    }
}
