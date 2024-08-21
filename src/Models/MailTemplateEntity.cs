// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


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
