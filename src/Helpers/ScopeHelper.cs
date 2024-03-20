using Aire.Id.Models;
using Aire.Sdk.Auth;

namespace Aire.Id.Helpers;

public static class ScopeHelper
{
    public static List<string> GetScopesForUser(UserEntity entity)
    {
        List<string> scopes = [];

        if (!entity.Verified)
            return [];

        if (!string.IsNullOrWhiteSpace(entity.Scopes)) // scope override
        {
            scopes = entity.Scopes
                    .Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
        }
        else // defaults + additional scopes
        {
            if (entity.Role != null
                && AireScopes.DefaultRoleScopes != null
                && AireScopes.DefaultRoleScopes.TryGetValue(entity.Role, out var roleScopes))
            {
                scopes.AddRange(roleScopes);
            }

            var additionalScopes = GetAdditionalScopesForUser(entity);

            if (additionalScopes != null)
                scopes.AddRange(additionalScopes);
        }

        return scopes;
    }

    public static List<string> GetBaseScopesForUser(UserEntity entity)
    {
        List<string> scopes = [];

        if (!entity.Verified)
            return [];

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

        return scopes;
    }

    public static List<string>? GetAdditionalScopesForUser(UserEntity entity)
    {
        return entity.AdditionalScopes?
            .Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();
    }
}
