namespace Aire.Sdk.Azure;

/// <summary>
/// Code = PartitionKey (first 5 chars), RowKey
/// </summary>
[EntityTable("AuthCodes")]
public class AuthCodeEntity : BaseTableEntity
{
    public string? ClientId { get; set; }
    public string? UserId { get; set; }
    public string? UserKey { get; set; }
    public string? Scopes { get; set; }
    public string? State { get; set; }
    public string? RedirectUri { get; set; }
    public DateTime? Expires { get; set; }

    public AuthCodeEntity() { }
    public AuthCodeEntity(string code)
    {
        PartitionKey = code[..5];
        RowKey = code;
    }
}
