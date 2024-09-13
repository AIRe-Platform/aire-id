// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Id.Models;
using Aire.Sdk.Auth;

namespace Aire.Id.Helpers;

public static class ScopeHelper
{
    public static List<string> GetScopesForUser(UserEntity entity)
    {
        List<string> scopes = GetBaseScopesForUser(entity);

        if(string.IsNullOrWhiteSpace(entity.Scopes)) // if not overridden
        {
            var additionalScopes = GetAdditionalScopesForUser(entity);

            if (additionalScopes != null)
                scopes.AddRange(additionalScopes);
        }

        return scopes;
    }

    public static List<string> GetBaseScopesForUser(UserEntity entity)
    {
        List<string> scopes = [];

        if (!string.IsNullOrWhiteSpace(entity.Scopes)) // scope override
        {
            scopes = entity.Scopes
                    .Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
        }
        else // defaults
        {
            if (entity.Role != null
                && AireScopes.DefaultRoleScopes != null
                && AireScopes.DefaultRoleScopes.TryGetValue(entity.Role, out var roleScopes))
            {
                scopes.AddRange(roleScopes);
            }
        }

        if (!entity.Verified) {
            if(scopes.Contains(AireScopes.PasswordChange))
                return [ AireScopes.PasswordChange ];
            else
                return [];
        }

        return scopes;
    }

    public static List<string>? GetAdditionalScopesForUser(UserEntity entity)
    {
        return entity.AdditionalScopes?
            .Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();
    }
}
