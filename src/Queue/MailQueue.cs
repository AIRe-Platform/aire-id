using Aire.Id.Models;
using Aire.Sdk.Azure;
using Azure.Communication.Email;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Aire.Id.Queue;

public class MailQueue
{
    private readonly ILogger<MailQueue> _logger;
    private readonly EmailClient _client;
    private readonly ITableStorageService _tables;

    private const string fallbackLocale = "en";

    public MailQueue(EmailClient client, ITableStorageService tables, ILogger<MailQueue> logger)
    {
        _client = client;
        _tables = tables;
        _logger = logger;
    }

    [Function(nameof(MailQueue))]
    public async Task Run([QueueTrigger(AireConstants.MailQueue, Connection = "StorageConnectionString")] MailTemplate mail)
    {
        if (string.IsNullOrEmpty(mail.TemplateName))
        {
            _logger.LogError("Missing mail template name");
            return;
        }

        _logger.LogInformation($"Sending email using template '{mail.TemplateName}'...");

        MailTemplateEntity? template = null;
        if (!string.IsNullOrEmpty(mail.Locale))
        {
            template = await _tables.RetrieveAsync<MailTemplateEntity>(mail.Locale, mail.TemplateName);
        }
        template ??= await _tables.RetrieveAsync<MailTemplateEntity>(fallbackLocale, mail.TemplateName);

        if (template == null)
        {
            throw new Exception($"Mail template '{mail.TemplateName}' not found.");
        }

        string html = template.HtmlContent!;
        string plainText = template.PlainTextContent!;

        if (mail.Values != null)
        {
            foreach (var val in mail.Values)
            {
                html = html.Replace($"{{{{{val.Key}}}}}", val.Value);
                plainText = plainText.Replace($"{{{{{val.Key}}}}}", val.Value);
            }
        }

        var operation = _client.Send(
            Azure.WaitUntil.Completed,
            senderAddress: AireEnvironment.EmailSenderAddress,
            recipientAddress: mail.Recipient,
            subject: template.Subject,
            htmlContent: html,
            plainTextContent: plainText);

        _logger.LogInformation($"Status: {operation.Value.Status}");
        _logger.LogInformation($"Operation id = {operation.Id}");
    }
}
