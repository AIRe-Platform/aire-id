using Aire.Sdk.Azure;
using Aire.Sdk.Helpers;
using Aire.Id.Models;
using Aire.Id.Oauth2.Models;
using Aire.Id.Oauth2.Providers;
using Aire.Sdk.Auth.Roles;
using Microsoft.Extensions.Logging;
using Aire.Sdk.Auth.Claims;

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

        public async Task<OauthSubject?> GetUser(string username, string password)
        {
            var hash = Crypto.SHA256Base16(username);

            var query = await _storage.QueryAsync<UserEntity>(x => x.EmailHash == hash);
            var user = await query.FirstOrDefaultAsync();

            if(user == null)
            {
                _log.LogWarning("User does not exist");
                return null;
            }

            if(!user.CheckPassword(password))
            {
                _log.LogWarning("Incorrect password");
                return null;
            }

            var key = user.GetEncryptionKey(password);
            var privateData = user.GetPrivateUserData(key!);

            var subject = new OauthSubject {
                Subject = user.UUID,
                Role = user.Role ?? AireRoles.User,
                Scopes = user.Scopes?
                    .Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .ToList(),
                Claims = new Dictionary<string, object> {
                    { AireClaims.UserEncryptionKey,  key! },
                    { AireClaims.ConnectedServices, privateData!.ConnectedServices! }
                },
                Verified = user.Verified
            };

            user.LastLogin = DateTime.UtcNow;
            if(!await _storage.UpsertAsync(user))
            {
                _log.LogError("Failed to update user last login!");
                return null;
            }

            return subject;
        }
    }
}