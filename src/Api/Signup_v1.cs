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
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Aire.Id
{
    public class Signup_v1
    {
        private readonly ITableStorageService _storage;
        private readonly QueueClient _mail_queue;
        private readonly ILogger<Signup_v1> _log;

        public Signup_v1(ITableStorageService storage, IAzureClientFactory<QueueServiceClient> clientFactory, ILogger<Signup_v1> log)
        {
            _storage = storage;
            _mail_queue = clientFactory
                .CreateClient("queue-client")
                .GetQueueClient(AireConstants.MailQueue);
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
            // TODO: Return error codes
            
            var request = await req.ReadFromJsonAsync<SignupRequest>();

            if(request == null)
                return new BadRequestResult();

            if(!Validation.ValidateSignupCredentials(request))
                return new BadRequestResult();

            var hash = Crypto.SHA256Base16(request.Credentials!.Email!);
            var query = await _storage.QueryAsync<UserEntity>(x => x.EmailHash == hash);
            var ent = await query.FirstOrDefaultAsync();

            if(ent != null)
                return new BadRequestResult();

            var uuid = Guid.NewGuid().ToString();
            var pw = request.Credentials!.Password!;
            
            var user = new UserEntity() { 
                UUID = uuid,
                EmailHash = hash,
                Role = AireRoles.User
            };
            user.GenerateEncryptionKey(uuid, pw);
            user.ChangePassword(null, pw);

            string verificationCode = user.GenerateVerificationCode();

            var key = user.GetEncryptionKey(pw);
            var userData = new User {
                Email = request.Credentials.Email
            };
            user.SetPrivateUserData(userData, key!);

            {
                bool result = await _storage.UpsertAsync(user);
                if(!result)
                    return new InternalServerErrorResult();
            }

            var mail = new MailTemplate 
            {
                Locale = userData.Language,
                Recipient = request.Credentials.Email,
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

