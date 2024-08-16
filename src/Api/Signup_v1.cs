// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using System.Net;
using System.Web.Http;
using Aire.Id.Models;
using Aire.Id.Helpers;
using Aire.Sdk.Azure;
using Aire.Sdk.Helpers;
using Aire.Sdk.Auth;
using Aire.Sdk.Models.Identity;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Aire.Sdk.AspNetCore;

namespace Aire.Id.Api;

public class Signup_v1
{
    private readonly ITableStorageService _storage;
    private readonly QueueClient _mail_queue;
    private readonly ILogger<Signup_v1> _log;

    public Signup_v1(ITableStorageService storage, QueueServiceClient queues, ILogger<Signup_v1> log)
    {
        _storage = storage;
        _mail_queue = queues.GetQueueClient(AireConstants.Queues.Mail);
        _mail_queue.CreateIfNotExists();
        _log = log;
    }

    [Function("Signup_v1")]
    [OpenApiOperation(
        operationId: "signup",
        tags: ["Sign-up"],
        Summary = "Register new user")]
    [OpenApiRequestBody("application/json", typeof(SignupRequest), Description = "Signup request", Required = true)]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Signup success")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid email format, account already exists, or password does not meet minimum requirements")]
    [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Internal error, try again later.")]
    public async Task<IActionResult> Signup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/signup")] HttpRequest req)
    {
        var request = await req.ReadJson<SignupRequest>();

        if (request == null)
            return new BadRequestResult();

        if (!Validation.ValidateSignupCredentials(request))
            return new BadRequestResult();

        var hash = Crypto.SHA256Base16(request.Credentials!.Email!);
        var query = await _storage.QueryAsync<UserEntity>(x => x.EmailHash == hash);
        var ent = await query.FirstOrDefaultAsync();

        if (ent != null)
            return new BadRequestResult();

        var pw = request.Credentials!.Password!;
        var user = new UserEntity()
        {
            EmailHash = hash,
            Role = AireRoles.User
        };
        user.GenerateEncryptionKey(pw);
        user.ChangePassword(null, pw);

        string verificationCode = user.GenerateVerificationCode();

        var key = user.GetEncryptionKey(pw);
        var userData = new User
        {
            Email = request.Credentials.Email
        };
        user.SetPrivateUserData(userData, key!);

        {
            bool result = await _storage.UpsertAsync(user);
            if (!result)
                return new InternalServerErrorResult();
        }

        var mail = new MailTemplate
        {
            Locale = userData.Language,
            Recipient = request.Credentials.Email,
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

