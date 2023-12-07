using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Aire.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Aire.Id.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly ILogger<JwtTokenService> _log;
        private readonly JwtSecurityTokenHandler _handler;
        private readonly TokenValidationParameters _validationParams;

        public JwtTokenService(
            JwtSecurityTokenHandler handler, TokenValidationParameters validationParams,
            ILogger<JwtTokenService> log)
        {
            _handler = handler;
            _validationParams = validationParams;
            _log = log;
        }

        public bool CheckAuthorization(JwtAuthFeature auth, string allowedRoles = null, string requiredScopes = null)
        {
            if(auth == null) return false;

            if(allowedRoles != null)
            {
                var allowed = allowedRoles.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var matchingRole = allowed.FirstOrDefault(x => auth.Principal.IsInRole(x));
                if(matchingRole == null)
                {
                    _log.LogWarning($"User does not have appropriate role to access this resource");
                    return false;
                }
            }

            if(requiredScopes != null)
            {
                var grantedScopes = auth.Principal.Claims.FirstOrDefault(x => x.Type == "scope");
                if(grantedScopes == null)
                    return false;
                var scopeValues = grantedScopes.Value.Split(" ");
                var required = requiredScopes.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                foreach(var scope in required)
                {
                    if(!scopeValues.Contains(scope))
                    {
                        _log.LogWarning($"Missing scope '{scope}'");
                        return false;
                    }
                }
            }

            return true;
        }

        public string IssueNewToken(string subject, string role, List<string> scopes, Dictionary<string, object> claims, TimeSpan lifetime)
        {
            var signingKeyBytes = Encoding.ASCII.GetBytes(AireEnvironment.TokenSigningKey);
            var signingKey = new SymmetricSecurityKey(signingKeyBytes);

            var encKeyBytes = Encoding.ASCII.GetBytes(AireEnvironment.TokenEncryptionKey);
            var encKey = new SymmetricSecurityKey(encKeyBytes);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new("sub", subject),
                    new(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow + lifetime,
                Issuer = AireEnvironment.TokenIssuer,
                Audience = AireEnvironment.TokenAudience,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature),
                EncryptingCredentials = new EncryptingCredentials(encKey, SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512),
                Claims = claims
            };

            if(scopes != null)
                descriptor.Claims.Add("scope", string.Join(" ", scopes));

            var token = _handler.CreateJwtSecurityToken(descriptor);
            return _handler.WriteToken(token);
        }

        public JwtSecurityToken ValidateToken(string token)
        {
            try
            {
                _handler.ValidateToken(token, _validationParams, out SecurityToken securityToken);
                return (JwtSecurityToken) securityToken;
            }
            catch
            {
                return null;
            }
        }
    }
}
