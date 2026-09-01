// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


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
    public async Task Run([QueueTrigger(AireConstants.Queues.Mail, Connection = "StorageConnectionString")] MailTemplate mail)
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

        var operation = await _client.SendAsync(
            Azure.WaitUntil.Completed,
            senderAddress: AireIdEnvironment.EmailSenderAddress,
            recipientAddress: mail.Recipient,
            subject: template.Subject,
            htmlContent: html,
            plainTextContent: plainText);

        var result = await operation.WaitForCompletionAsync();

        if (result.Value.Status != EmailSendStatus.Succeeded)
        {
            _logger.LogError($"Status: {result.Value.Status}");
            _logger.LogError($"Operation id = {operation.Id}");

            throw new Exception($"(${operation.Id}) Send failed: {result.Value.Status}");
        }
        else
        {
            _logger.LogInformation($"Status: {result.Value.Status}");
            _logger.LogInformation($"Operation id = {operation.Id}");
        }

    }
}
