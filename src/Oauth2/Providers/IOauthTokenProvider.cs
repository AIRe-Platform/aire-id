using Aire.Id.Oauth2.Models;

namespace Aire.Id.Oauth2.Providers
{
    public interface IOauthTokenProvider
    {
        string IssueNewToken(OauthTokenDescription description);
        OauthTokenResponse? GetTokenInfo(string token);
    }
}
