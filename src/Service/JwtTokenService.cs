using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Aire.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;


namespace Aire.Id.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly ILogger<JwtTokenService> _log;
        private readonly IHttpContextAccessor _http;
        private readonly JwtSecurityTokenHandler _handler;
        private readonly TokenValidationParameters _validationParams;

        public JwtTokenService(ILogger<JwtTokenService> log, IHttpContextAccessor httpContext, JwtSecurityTokenHandler handler)
        {
            _log = log;
            _http = httpContext;
            _handler = handler;

            var keyBytes = Encoding.ASCII.GetBytes(AireEnvironment.TokenSigningKey);

            _validationParams = new TokenValidationParameters()
            {
                RequireSignedTokens = true,

                ValidAudience = AireEnvironment.TokenAudience,
                ValidIssuer = AireEnvironment.TokenIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true
            };
        }

        public JwtSecurityToken ValidateRequestToken(string allowedRoles = null, string requiredScopes = null)
        {
            if(_http == null) return null;
            if(_http.HttpContext == null) return null;
            if(_http.HttpContext.Request == null) return null;

            IHeaderDictionary headers = _http.HttpContext.Request.Headers;
            if(!headers.TryGetValue("Authorization", out var value))
            {
                _log.LogWarning("Missing Authorization header");
                return null;
            }

            var parts = ((string)value).Split(" ");
            if(parts[0] != "Bearer")
            {
                _log.LogWarning("Invalid Authorization scheme");
                return null;
            }

            if(parts.Length != 2)
            {
                _log.LogWarning("Invalid Authorization header format");
                return null;
            }

            try
            {
                ClaimsPrincipal principal = _handler.ValidateToken(parts[1], _validationParams, out var token);
                var jwt = (JwtSecurityToken) token;

                if(allowedRoles != null)
                {
                    var allowed = allowedRoles.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    var matchingRole = allowed.FirstOrDefault(x => principal.IsInRole(x));
                    if(matchingRole == null)
                    {
                        _log.LogWarning($"User does not have appropriate role to access this resource");
                        return null;
                    }
                }

                if(requiredScopes != null)
                {
                    var grantedScopes = principal.Claims.FirstOrDefault(x => x.Type == "scope");
                    if(grantedScopes == null)
                        return null;
                    var scopeValues = grantedScopes.Value.Split(" ");
                    var required = requiredScopes.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    foreach(var scope in required)
                    {
                        if(!scopeValues.Contains(scope))
                        {
                            _log.LogWarning($"Missing scope '{scope}'");
                            return null;
                        }
                    }
                }

                return jwt;
            }
            catch(Exception ex)
            {
                _log.LogWarning(ex, "Token validation failed");
                return null;
            }
        }

        public string IssueNewToken(string subject, string role, List<string> scopes, Dictionary<string, object> claims, TimeSpan lifetime)
        {
            var keyBytes = Encoding.ASCII.GetBytes(AireEnvironment.TokenSigningKey);
            var signingKey = new SymmetricSecurityKey(keyBytes);

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
                Claims = claims
            };

            var token = _handler.CreateJwtSecurityToken(descriptor);
            return _handler.WriteToken(token);
        }
    }
}
