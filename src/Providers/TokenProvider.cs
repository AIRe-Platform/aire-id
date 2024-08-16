// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Id.Oauth2.Models;
using Aire.Id.Oauth2.Providers;
using Aire.Sdk.Auth;
using Microsoft.Extensions.Logging;

namespace Aire.Id.Providers;

public class TokenProvider : IOauthTokenProvider
{
    private readonly IJwtTokenService _jwt;
    private readonly ILogger<TokenProvider> _log;

    public TokenProvider(IJwtTokenService jwt, ILogger<TokenProvider> log)
    {
        _jwt = jwt;
        _log = log;
    }

    public OauthTokenResponse? GetTokenInfo(string token)
    {
        var securityToken = _jwt.ValidateToken(token);
        if (securityToken == null)
            return null;

        var scope = securityToken.Claims.FirstOrDefault(x => x.Type == "scope")?.Value;

        var response = new OauthTokenResponse()
        {
            TokenType = OauthTokenType.Bearer,
            AccessToken = token,
            ExpiresIn = (int)(securityToken.ValidTo - securityToken.ValidFrom).TotalSeconds,
            Scope = scope
        };

        return response;
    }

    public string IssueNewToken(OauthTokenDescription description)
    {
        return _jwt.IssueNewToken(
            description.Subject!.Subject!,
            description.Subject!.Role!,
            description.Subject!.Scopes!,
            description.Subject!.Claims!,
            description.Lifetime
        );
    }
}
