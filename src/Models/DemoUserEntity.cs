using Aire.Sdk.Azure;

namespace Aire.Id.Models;

[EntityTable("Users")]
public class DemoUserEntity : UserEntity
{
    public string? DemoGroupId { get; set; }
    public string? DemoAccessCode { get; set; }
}
