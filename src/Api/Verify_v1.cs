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
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Aire.Id.Api;

public class Verify_v1
{
    private readonly ITableStorageService _storage;
    private readonly QueueClient _mail_queue;
    private readonly ILogger<Verify_v1> _log;

    public Verify_v1(
        ITableStorageService storage,
        QueueServiceClient queues,
        ILogger<Verify_v1> log)
    {
        _storage = storage;
        _mail_queue = queues.GetQueueClient(AireConstants.Queues.Mail);
        _log = log;
    }

    [Function("Verify_v1")]
    [OpenApiOperation(
        operationId: "verifyCode",
        tags: ["Verification"],
        Summary = "Activate user account with a verification code")]
    [OpenApiParameter("code", Description = "Verification code", In = ParameterLocation.Path, Required = true)]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Verification succeeded")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Invalid or expired code")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or invalid user token")]
    public async Task<IActionResult> VerifyCode(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/verify/{code}")] HttpRequest req,
        string code,
        FunctionContext context)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (auth.VerifiedAccount)
            return new NoContentResult();

        string id = auth!.Token.Subject;
        var user = await _storage.RetrieveAsync<UserEntity>(id);

        if (user == null)
            return new UnauthorizedResult();

        bool verified = user.VerifyAccount(code);

        bool result = await _storage.UpsertAsync(user);
        if (!result)
            return new InternalServerErrorResult();

        if (verified)
        {
            return new NoContentResult();
        }
        else
        {
            _log.LogWarning("Verification failed");
            return new ForbiddenResult();
        }
    }

    [Function("ResendVerify_v1")]
    [OpenApiOperation(
        operationId: "resendVerify",
        tags: ["Verification"],
        Summary = "Re-send the verification email")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Success")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Already verified")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or invalid user token")]
    public async Task<IActionResult> ResendVerify(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/verify/resend")] HttpRequest req,
        FunctionContext context)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (auth.VerifiedAccount)
            return new BadRequestResult();

        string id = auth!.Token.Subject;
        var user = await _storage.RetrieveAsync<UserEntity>(id);

        if (user == null)
            return new UnauthorizedResult();

        string verificationCode = user.GenerateVerificationCode();

        {
            bool result = await _storage.UpsertAsync(user);
            if (!result)
                return new InternalServerErrorResult();
        }

        var userData = user.GetPrivateUserData(auth.UserKey);
        if (userData == null)
            return new UnauthorizedResult();

        var mail = new MailTemplate
        {
            Locale = userData.Language,
            Recipient = userData.Email,
            TemplateName = MailTemplate.Verification.Id,
            Values = new Dictionary<string, string> {
                { MailTemplate.Verification.Params.Code, verificationCode }
            }
        };

        {
            var result = await _mail_queue.SendMessageAsync(mail.ObjectToJson());
            _log.LogInformation($"Queued verification mail. MessageId: {result.Value.MessageId}");
        }

        return new NoContentResult();
    }
}
