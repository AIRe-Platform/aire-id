using System.Runtime.Serialization;
using Aire.Sdk.Azure;
using Aire.Sdk.Helpers;
using Aire.Sdk.Models.Demo;
using Aire.Services.Models;

namespace Aire.Id.Models;

[EntityTable("DemoGroup")]
public class DemoGroupEntity : BaseTableEntity
{
    [IgnoreDataMember]
    public string? Id
    {
        get => base.RowKey;
        set
        {
            base.RowKey = value;
            base.PartitionKey = value;
        }
    }

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
}
