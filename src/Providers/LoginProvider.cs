// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Sdk.Azure;
using Aire.Sdk.Helpers;
using Aire.Sdk.Auth;
using Aire.Id.Models;
using Aire.Id.Oauth2.Models;
using Aire.Id.Oauth2.Providers;
using Microsoft.Extensions.Logging;

namespace Aire.Id.Providers;

public class LoginProvider : IOauthLoginProvider
{
    private readonly ITableStorageService _storage;
    private readonly ILogger<LoginProvider> _log;

    public LoginProvider(ITableStorageService storage, ILogger<LoginProvider> log)
    {
        _storage = storage;
        _log = log;
    }

    public async Task<OauthSubject?> Login(string username, string password)
    {
        var hash = Crypto.SHA256Base16(username);

        var query = await _storage.QueryAsync<UserEntity>(x => x.EmailHash == hash || x.Username == username);
        var user = await query.FirstOrDefaultAsync();

        if (user == null)
        {
            _log.LogWarning("User does not exist");
            return null;
        }

        if (!user.CheckPassword(password))
        {
            _log.LogWarning("Incorrect password");
            return null;
        }

        if (user.HasRole(AireRoles.DemoUser))
        {
            var asDemoUser = await _storage.RetrieveAsync<DemoUserEntity>(user.UUID());
            if (asDemoUser?.DemoGroupId != null)
            {
                var group = await _storage.RetrieveAsync<DemoGroupEntity>(asDemoUser.DemoGroupId);
                if (group == null || group.Active == false)
                {
                    _log.LogWarning("The demo user is deactivated");
                    return null;
                }
            }
        }

        if (user.HasRole(AireRoles.TrialUser))
        {
            _log.LogWarning("Logins disabled for trial users");
            return null;
        }

        var key = user.GetEncryptionKey(password)!;
        var subject = GetSubject(user, key, null);

        user.LastLogin = DateTime.UtcNow;
        if (!await _storage.UpsertAsync(user))
        {
            _log.LogError("Failed to update user last login!");
            return null;
        }

        return subject;
    }

    public OauthSubject GetSubject(UserEntity user, string key, string? platform)
    {
        var privateData = user.GetPrivateUserData(key!);

        var subject = new OauthSubject
        {
            Subject = user.UUID(),
            Role = platform == null ? AireRoles.NonMember : user.GetRole(platform),
            AllowedScopes = platform == null ? [] : user.GetScopes(platform),
            Claims = new Dictionary<string, object> {
                    { AireClaims.UserEncryptionKey, key },
                    { AireClaims.ConnectedServices, privateData!.ConnectedServices! },
                    { AireClaims.VerifiedAccount, user.Verified ? "1" : "0" }
                },
            Verified = user.Verified
        };

        return subject;
    }
}
