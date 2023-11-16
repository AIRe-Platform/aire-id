
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aire.Helpers;
using Aire.Id.Models;
using Aire.Id.Oauth2.Models;
using Aire.Id.Oauth2.Providers;
using Microsoft.Extensions.Logging;

namespace Aire.Id.Providers
{
    public class LoginProvider : IOauthLoginProvider
    {
        private readonly ITableStorageService _storage;
        private readonly ILogger<LoginProvider> _log;

        public LoginProvider(ITableStorageService storage, ILogger<LoginProvider> log)
        {
            _storage = storage;
            _log = log;
        }

        public async Task<OauthSubject> GetUser(string username, string password)
        {
            var hash = Crypto.SHA256Base16(username);

            var query = await _storage.QueryAsync<UserEntity>(x => x.EmailHash == hash);
            var user = await query.FirstOrDefaultAsync();

            if(user == null)
                return null;

            // TODO: Add roles and scopes and other claims to the user entity
            var subject = new OauthSubject {
                Subject = user.UUID,
                Role = UserRoles.User,
                Scopes = new(),
                Claims = new Dictionary<string, object> {
                    { "user_enc_key", Crypto.DeriveUserEncryptionKey(user.UUID, password) }
                }
            };

            return subject;
        }
    }
}