// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using System.Net;
using System.Web.Http;
using Aire.Id.Models;
using Aire.Sdk.Azure;
using Aire.Sdk.Auth;
using Aire.Sdk.AspNetCore;
using Aire.Sdk.Helpers;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Aire.Id.Api;

public class Recovery_v1
{
    private readonly ITableStorageService _storage;
    private readonly QueueClient _mail_queue;
    private readonly ILogger<Recovery_v1> _log;

    public Recovery_v1(
        ITableStorageService storage,
        QueueServiceClient queues,
        ILogger<Recovery_v1> log)
    {
        _storage = storage;
        _mail_queue = queues.GetQueueClient(AireConstants.Queues.Mail);
        _log = log;
    }

    [Function("RecoveryCode_v1")]
    [OpenApiOperation(
        operationId: "requestRecoveryCode",
        tags: ["Recovery"],
        Summary = "Request a password recovery code to be sent in the email")]
    [OpenApiRequestBody("application/json", typeof(RecoveryCodeRequest), Description = "Password recovery code request body", Required = true)]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Returned always whether an account is found or not.")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotImplemented, Description = "Account recovery is not supported by the platform.")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid request.")]
    public async Task<IActionResult> RequestRecoveryCode(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/recovery/code")] HttpRequest req)
    {
        if (string.IsNullOrWhiteSpace(AireIdEnvironment.GlobalRecoveryKey))
            return new StatusCodeResult((int)HttpStatusCode.NotImplemented);

        var body = await req.ReadJson<RecoveryCodeRequest>();
        if (body == null || !body.Validate(true))
        {
            _log.LogError("Invalid request");
            return new BadRequestResult();
        }

        var hash = Crypto.SHA256Base16(body.Email!);
        var query = await _storage.QueryAsync<UserEntity>(x => x.EmailHash == hash);
        var user = await query.FirstOrDefaultAsync();

        if (user == null)
        {
            _log.LogWarning("Could not find the user account");
            return new NoContentResult();
        }

        var rights = user.GetAccessRights();
        if (!rights.Any(x => x.Value.GetScopes().Contains(AireScopes.PasswordChange)))
        {
            _log.LogWarning("This account is not allowed to change the password");
            return new NoContentResult();
        }

        string code = user.GenerateVerificationCode();
        {
            bool result = await _storage.UpsertAsync(user);
            if (!result)
                return new InternalServerErrorResult();
        }

        var mail = new MailTemplate
        {
            Locale = body.Language,
            Recipient = body.Email,
            TemplateName = MailTemplate.RecoveryCode.Id,
            Values = new Dictionary<string, string> {
                { MailTemplate.RecoveryCode.Params.Code, code }
            }
        };

        {
            var result = await _mail_queue.SendMessageAsync(mail.ObjectToJson());
            _log.LogInformation($"Queued account recovery mail. MessageId: {result.Value.MessageId}");
        }

        return new NoContentResult();
    }

    [Function("RecoveryPassword_v1")]
    [OpenApiOperation(
        operationId: "recoveryPasswordChange",
        tags: ["Recovery"],
        Summary = "Change the password using a recovery code")]
    [OpenApiRequestBody("application/json", typeof(RecoveryPasswordChangeRequest), Description = "Password recovery change request body", Required = true)]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Success")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid request.")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "The account does not exist")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotImplemented, Description = "Account recovery is not supported by the platform.")]
    [OpenApiResponseWithoutBody(HttpStatusCode.UnprocessableEntity, Description = "The account is not recoverable.")]
    public async Task<IActionResult> RecoveryPasswordChange(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/recovery/password")] HttpRequest req)
    {
        if (string.IsNullOrWhiteSpace(AireIdEnvironment.GlobalRecoveryKey))
            return new StatusCodeResult((int)HttpStatusCode.NotImplemented);

        var body = await req.ReadJson<RecoveryPasswordChangeRequest>();
        if (body == null || !body.Validate())
            return new BadRequestResult();

        var hash = Crypto.SHA256Base16(body.Email!);
        var query = await _storage.QueryAsync<UserEntity>(x => x.EmailHash == hash);
        var user = await query.FirstOrDefaultAsync();
        if (user == null)
        {
            _log.LogWarning("Account does not exist");
            return new NotFoundResult();
        }

        bool verified = user.VerifyAccount(body.Code!);
        if (!verified)
        {
            bool updated = await _storage.UpsertAsync(user);
            if (!updated)
                return new InternalServerErrorResult();

            _log.LogWarning("Failed to verify the code");
            return new BadRequestResult();
        }

        bool recovered = user.RecoverAccount(body.Password!);
        if (!recovered)
        {
            _log.LogWarning("This account cannot be recovered");
            return new UnprocessableEntityResult();
        }

        {
            bool updated = await _storage.UpsertAsync(user);
            if (!updated)
                return new InternalServerErrorResult();
        }

        var mail = new MailTemplate
        {
            Locale = body.Language,
            Recipient = body.Email,
            TemplateName = MailTemplate.PasswordChanged.Id
        };

        {
            var result = await _mail_queue.SendMessageAsync(mail.ObjectToJson());
            _log.LogInformation($"Queued password change notification mail. MessageId: {result.Value.MessageId}");
        }

        return new NoContentResult();
    }
}
