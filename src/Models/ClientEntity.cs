using Aire.Sdk.Models.Admin;

namespace Aire.Sdk.Azure;

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
            Active = Active
        };
    }
}
