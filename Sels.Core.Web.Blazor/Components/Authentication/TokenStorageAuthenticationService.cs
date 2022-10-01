using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Sels.Core.Web.Blazor.Authentication.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Blazor.Authentication
{
    /// <summary>
    /// Authentication state provider that manages state by saving auth tokens to the browser storage.
    /// </summary>
    public class TokenStorageAuthenticationService : AuthenticationStateProvider, IAuthenticationService
    {
        // Fields
        private readonly ITokenService _tokenService;
        private readonly ILocalStorageService _localStorageService;
        private readonly string _storageKey;
        private readonly ILogger<TokenStorageAuthenticationService>? _logger;

        /// <summary>
        /// Constructor for proxy generation.
        /// </summary>
        public TokenStorageAuthenticationService()
        {

        }

        /// <inheritdoc cref="TokenStorageAuthenticationService"/>
        /// <param name="tokenService">Service used to validate the tokens</param>
        /// <param name="localStorageService">Service used to access the tokens saved in the browser storage</param>
        /// <param name="storageKey">The key used to retrieve the tokens from storage</param>
        /// <param name="logger">Optional logger for tracing</param>
        public TokenStorageAuthenticationService(ITokenService tokenService, ILocalStorageService localStorageService, string storageKey, ILogger<TokenStorageAuthenticationService>? logger = null)
        {
            _tokenService = tokenService.ValidateArgument(nameof(tokenService));
            _localStorageService = localStorageService.ValidateArgument(nameof(localStorageService));
            _storageKey = storageKey.ValidateArgumentNotNullOrWhitespace(nameof(storageKey));
            _logger = logger;
        }

        /// <inheritdoc/>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            _logger.Log($"Getting authentication state from browser storage");
            var token = await _localStorageService.GetItemAsync<string>(_storageKey);

            if (token.HasValue())
            {
                _logger.Debug($"Got token from browser storage under key <{_storageKey}>. Attempting to extract principal");

                if(await _tokenService.ValidateTokenAsync(token))
                {
                    return new AuthenticationState(await _tokenService.GetPrincipal(token));
                }
                else
                {
                    _logger.Warning($"Auth token <{token}> is no longer valid. This could be because it expired or was tampered with. Removing from storage");
                    await _localStorageService.RemoveItemAsync(_storageKey);
                }
            }
            else
            {
                _logger.Debug($"No token set in the browser storage under key <{_storageKey}>");
            }
            return new AuthenticationState(new ClaimsPrincipal());
        }

        /// <inheritdoc/>
        public async Task SignIn(ClaimsPrincipal principal, DateTime? expiryDate = null)
        {
            principal.ValidateArgument(nameof(principal));

            _logger.Log($"Signing in principal <{principal}>");
            _logger.Debug($"Generating token for principal <{principal}>");
            var token = await _tokenService.CreateTokenAsync(principal.Claims, expiryDate);
            _logger.Debug($"Generated token for principal <{principal}>. Setting storage");

            await _localStorageService.SetItemAsync<string>(_storageKey, token);
        }
        /// <inheritdoc/>
        public async Task SignOut()
        {
            _logger.Log($"Clearing any authentication state");

            await _localStorageService.RemoveItemAsync(_storageKey);
        }
    }
}
