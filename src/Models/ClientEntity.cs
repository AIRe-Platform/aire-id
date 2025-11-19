// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using System.Collections;
using Aire.Sdk.Azure;
using Aire.Sdk.Models.Admin;

namespace Aire.Id.Models;

/// <summary>
/// Client ID = PartitionKey, RowKey
/// </summary>
[EntityTable("Clients")]
public class ClientEntity : BaseTableEntity
{
    public string? Name { get; set; }
    public string? AllowedScopes { get; set; }
    public string? RedirectUri { get; set; }
    public bool Active { get; set; }
    public bool Public { get; set; }
    public bool RequireConsent { get; set; }
    public string? GrantTypes { get; set; }
    public string? SecretHash { get; set; }
    public bool RequirePlatform { get; set; }
    public string? Platforms { get; set; }

    public ClientEntity()
    {
        string id = Guid.NewGuid().ToString();
        PartitionKey ??= id;
        RowKey ??= id;
    }

    public ClientEntity(string id)
    {
        PartitionKey = id;
        RowKey = id;
    }

    public string Id()
    {
        return RowKey ?? "";
    }

    public List<string> GetAllowedScopes()
    {
        return [.. (AllowedScopes ?? "").Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
    }

    public void SetAllowedScopes(IEnumerable<string> scopes)
    {
        AllowedScopes = string.Join(" ", scopes);
    }

    public List<string> GetAllowedPlatforms()
    {
        return [.. (Platforms ?? "").Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
    }

    public void SetAllowedPlatforms(IEnumerable<string> platforms)
    {
        Platforms = string.Join(",", platforms);
    }

    public List<string> GetGrantTypes()
    {
        return [.. (GrantTypes ?? "").Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
    }

    public void SetGrantTypes(IEnumerable<string> grantTypes)
    {
        GrantTypes = string.Join(",", grantTypes);
    }

    public Client ToModel()
    {
        return new Client
        {
            Id = Id(),
            Name = Name,
            Scopes = GetAllowedScopes(),
            RedirectUri = RedirectUri,
            Active = Active,
            Public = Public,
            RequireConsent = RequireConsent,
            GrantTypes = GetGrantTypes(),
            RequirePlatform = RequirePlatform,
            Platforms = GetAllowedPlatforms()
        };
    }
}
