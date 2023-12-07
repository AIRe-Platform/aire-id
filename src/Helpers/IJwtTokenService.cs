using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Aire.Helpers
{
    public interface IJwtTokenService
    {
        JwtSecurityToken ValidateToken(string token);
        bool CheckAuthorization(JwtAuthFeature auth, string allowedRoles = null, string requiredScopes = null);
        string IssueNewToken(string subject, string role, List<string> scopes, Dictionary<string, object> claims, TimeSpan lifetime);
    }
}
