using System.Runtime.Serialization;
using Aire.Sdk.Azure;
using Aire.Sdk.Helpers;
using Aire.Sdk.Models.Demo;

namespace Aire.Id.Models;

/// <summary>
/// ID: PartitionKey, RowKey
/// </summary>
[EntityTable("DemoGroup")]
public class DemoGroupEntity : BaseTableEntity
{
    public string? Name { get; set; }
    public string? UsernamePrefix { get; set; }
    public string? UsersJson { get; set; }
    public bool? Active { get; set; }

    [IgnoreDataMember]
    public List<DemoUser>? Users
    {
        get => UsersJson?.JsonToObject<List<DemoUser>>();
        set => UsersJson = value.ObjectToJson();
    }

    public DemoGroupEntity()
    {
        string id = Guid.NewGuid().ToString();
        PartitionKey ??= id;
        RowKey ??= id;
    }

    public DemoGroupEntity(string id)
    {
        PartitionKey = id;
        RowKey = id;
    }

    public string Id()
    {
        return RowKey ?? "";
    }
}
