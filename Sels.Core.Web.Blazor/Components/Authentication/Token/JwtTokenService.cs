using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Sels.Core.Contracts.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Blazor.Authentication.Token
{
    /// <summary>
    /// Token service that generates jwt tokens.
    /// </summary>
    public class JwtTokenService : ITokenService
    {
        // Fields
        private readonly ILogger<JwtTokenService>? _logger;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly byte[] _secret;

        /// <inheritdoc cref="JwtTokenService"/>
        /// <param name="issuer">The issuer of the jwt tokens</param>
        /// <param name="audience">The services that accept the jwt tokens</param>
        /// <param name="secret">Bytes used to sign the jwt token</param>
        /// <param name="logger">Optional logger for tracing</param>
        public JwtTokenService(string issuer, string audience, byte[] secret, ILogger<JwtTokenService>? logger = null)
        {
            _issuer = issuer.ValidateArgumentNotNullOrWhitespace(nameof(issuer));
            _audience = audience.ValidateArgumentNotNullOrWhitespace(nameof(audience));
            _secret = secret.ValidateArgumentNotNullOrEmpty(nameof(secret));
            _logger = logger;
        }

        /// <inheritdoc/>
        public Task<string> CreateTokenAsync(IEnumerable<Claim> claims, DateTime? expiryDate = null)
        {
            claims.ValidateArgumentNotNullOrEmpty(nameof(claims));
            _logger.Log($"Generating jwt token for <{claims.Count()}> claims");

            var key = new SymmetricSecurityKey(_secret);
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _issuer,
                _audience,
                claims,
                expires: expiryDate ?? DateTime.Now.AddYears(1),
                signingCredentials: credentials
                );

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }
        /// <inheritdoc/>
        public async Task<Claim[]> GetClaims(string token)
        {
            token.ValidateArgumentNotNullOrWhitespace(nameof(token));
            _logger.Log($"Getting claims from token");

            if (await ValidateTokenAsync(token))
            {
                return GetPrincipalFromToken(token).Claims.ToArray();
            }

            return Array.Empty<Claim>();
        }
        /// <inheritdoc/>
        public async Task<ClaimsPrincipal?> GetPrincipal(string token)
        {
            token.ValidateArgumentNotNullOrWhitespace(nameof(token));
            _logger.Log($"Getting principal from token");

            if (await ValidateTokenAsync(token))
            {
                return GetPrincipalFromToken(token);
            }

            return null;
        }

        /// <inheritdoc/>
        public Task<bool> ValidateTokenAsync(string token)
        {
            token.ValidateArgumentNotNullOrWhitespace(nameof(token));

            _logger.Log($"Validating token");

            return Task.FromResult(GetPrincipalFromToken(token) != null);
        }

        private ClaimsPrincipal? GetPrincipalFromToken(string token)
        {
             token.ValidateArgumentNotNullOrWhitespace(nameof(token));

            var parameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(_secret)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                return tokenHandler.ValidateToken(token, parameters, out var validatedToken);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
