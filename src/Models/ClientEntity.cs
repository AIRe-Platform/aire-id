// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


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

    public Client ToModel()
    {
        return new Client
        {
            Id = Id(),
            Name = Name,
            Scopes = AllowedScopes?
                .Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList(),
            RedirectUri = RedirectUri,
            Active = Active,
            Public = Public,
            RequireConsent = RequireConsent,
            GrantTypes = GrantTypes?
                .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList()
        };
    }
}
