using Aire.Sdk.Azure;

namespace Aire.Id.Models;

/// <summary>
/// Locale: PartitionKey
/// Name: RowKey
/// </summary>
[EntityTable("MailTemplate")]
public class MailTemplateEntity : BaseTableEntity
{
    public string? Subject { get; set; }
    public string? HtmlContent { get; set; }
    public string? PlainTextContent { get; set; }

    public MailTemplateEntity() { }
    public MailTemplateEntity(string locale, string name)
    {
        PartitionKey = locale;
        RowKey = name;
    }
}
