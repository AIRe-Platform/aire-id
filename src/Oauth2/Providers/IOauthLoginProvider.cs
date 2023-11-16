using System.Threading.Tasks;
using Aire.Id.Oauth2.Models;

namespace Aire.Id.Oauth2.Providers
{
    public interface IOauthLoginProvider
    {
        abstract Task<OauthSubject> GetUser(string username, string password);
    }
}