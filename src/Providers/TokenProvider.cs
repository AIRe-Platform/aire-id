using Aire.Helpers;
using Aire.Id.Oauth2.Models;
using Aire.Id.Oauth2.Providers;
using Microsoft.Extensions.Logging;

namespace Aire.Id.Providers
{
    public class TokenProvider : IOauthTokenProvider
    {
        private readonly IJwtTokenService _jwt;
        private readonly ILogger<TokenProvider> _log;

        public TokenProvider(IJwtTokenService jwt, ILogger<TokenProvider> log)
        {
            _jwt = jwt;
            _log = log;
        }

        public string IssueNewToken(OauthTokenDescription description)
        {
            return _jwt.IssueNewToken(
                description.Subject.Subject,
                description.Subject.Role,
                description.Subject.Scopes,
                description.Subject.Claims,
                description.Lifetime
            );
        }
    }
}