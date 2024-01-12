using System.Runtime.Serialization;
using Aire.Sdk.Azure;
using Aire.Services.Models;

namespace Aire.Id.Models
{
    [EntityTable("MailTemplate")]
    public class MailTemplateEntity : BaseTableEntity
    {
        [IgnoreDataMember]
        public string? Locale { get => PartitionKey; set => PartitionKey = value; }

        [IgnoreDataMember]
        public string? TemplateName { get => RowKey; set => RowKey = value; }

        public string? Subject { get; set; }
        public string? HtmlContent { get; set; }
        public string? PlainTextContent { get; set; }
    }
}