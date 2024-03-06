using Aire.Id.Models;
using Aire.Id.Oauth2.Models;

namespace Aire.Id.Oauth2.Providers
{
    public interface IOauthLoginProvider
    {
        public abstract Task<OauthSubject?> Login(string username, string password);
        public OauthSubject GetSubject(UserEntity user, string key);
    }
}
