using System.Net;
using Aire.Sdk.Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Aire.Sdk.Auth.Models;
using Aire.Sdk.Auth.Scopes;
using Aire.Sdk.Auth.Roles;
using Aire.Sdk.Auth.Services;
using Aire.Id.Models;
using System.Web.Http;
using Aire.Sdk.AspNetCore;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Azure;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Aire.Sdk.Helpers;

namespace Aire.Id
{
    public class Verify_v1
    {
        private readonly ITableStorageService _storage;
        private readonly IJwtTokenService _jwt;
        private readonly QueueClient _mail_queue;
        private readonly ILogger<Verify_v1> _log;

        public Verify_v1(
            ITableStorageService storage, 
            IJwtTokenService jwt, 
            IAzureClientFactory<QueueServiceClient> clientFactory, 
            ILogger<Verify_v1> log)
        {
            _storage = storage;
            _jwt = jwt;
            _mail_queue = clientFactory.CreateClient("queue-client").GetQueueClient("mail-queue");
            _log = log;
        }

        [Function("Verify_v1")]
        [OpenApiOperation(
            operationId: "verifyCode", 
            tags: ["User sign-up and verification"], 
            Summary = "Activate user account with a verification code")]
        [OpenApiParameter("code", Description = "Verification code", In = ParameterLocation.Path, Required = true)]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Verification succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Invalid or expired code")]
        [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Already verified")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or invalid user token")]
        public async Task<IActionResult> VerifyCode(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/verify/{code}")] HttpRequestData req,
            string code,
            FunctionContext context)
        {
            var auth = context.Features.Get<JwtAuthFeature>();
            if(!_jwt.CheckAuthorization(auth, AireRoles.User, AireScopes.UnverifiedAccount))
                return new UnauthorizedResult();

            string id = auth!.Token.Subject;
            var user = await _storage.RetrieveAsync<UserEntity>(id);

            if(user == null)
                return new UnauthorizedResult();

            if(user.Verified)
                return new BadRequestResult();

            if(DateTime.UtcNow < user.VerificationCodeExpiry && user.VerificationCodeRetryCount < 10)
            {
                if(user.VerificationCode == code)
                {
                    user.Verified = true;
                    user.VerificationCode = null;
                    user.VerificationCodeExpiry = null;
                    user.VerificationCodeRetryCount = null;
                }
                else
                {
                    user.VerificationCodeRetryCount++;
                    _log.LogWarning("Incorrect verification code");
                }
            }
            else
            {
                _log.LogWarning("Code expired or too many retries");
            }

            bool result = await _storage.UpsertAsync(user);
            if(!result)
                return new InternalServerErrorResult();

            if(user.Verified)
                return new NoContentResult();
            else
                return new ForbiddenResult();
        }

        [Function("ResendVerify_v1")]
        [OpenApiOperation(
            operationId: "resendVerify", 
            tags: ["User sign-up and verification"], 
            Summary = "Re-send the verification email")]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Success")]
        [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Already verified")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or invalid user token")]
        public async Task<IActionResult> ResendVerify(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/verify/resend")] HttpRequest req,
            FunctionContext context)
        {            
            var auth = context.Features.Get<JwtAuthFeature>();
            if(!_jwt.CheckAuthorization(auth, AireRoles.User, AireScopes.UnverifiedAccount))
                return new UnauthorizedResult();

            string id = auth!.Token.Subject;
            var user = await _storage.RetrieveAsync<UserEntity>(id);

            if(user == null)
                return new UnauthorizedResult();
            
            string verificationCode = user.GenerateVerificationCode();

            {
                bool result = await _storage.UpsertAsync(user);
                if(!result)
                    return new InternalServerErrorResult();
            }
                
            var userData = user.GetPrivateUserData(auth.UserKey);
            if(userData == null)
                return new UnauthorizedResult();

            var mail = new MailTemplate 
            {
                Locale = userData.Language,
                Recipient = userData.Email,
                TemplateName = "verification",
                Values = new Dictionary<string, string> {
                    { "code", verificationCode }
                }
            };

            {
                var result = await _mail_queue.SendMessageAsync(mail.ObjectToJson());
                _log.LogInformation($"Queued verification mail. MessageId: {result.Value.MessageId}");
            }

            return new NoContentResult();
        }
    }
}

