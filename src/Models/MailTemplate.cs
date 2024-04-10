namespace Aire.Id.Models;

public class MailTemplate
{
    public string? TemplateName { get; set; }
    public string? Locale { get; set; }
    public string? Recipient { get; set; }
    public Dictionary<string, string>? Values { get; set; }
}
