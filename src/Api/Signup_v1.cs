using System.Net;
using System.Web.Http;
using Aire.Sdk.TableStorage;
using Aire.Sdk.AspNetCore;
using Aire.Sdk.Helpers;
using Aire.Id.Models;
using Aire.Id.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Aire.Id
{
    public class Signup_v1
    {
        private readonly ITableStorageService _storage;
        private readonly ILogger<Signup_v1> _log;

        public Signup_v1(ITableStorageService storage, ILogger<Signup_v1> log)
        {
            _storage = storage;
            _log = log;
        }

        [Function("Signup_v1")]
        [OpenApiOperation(operationId: "Run", tags: ["User sign-up and verification"], Description = "Register a new user")]
        [OpenApiRequestBody("application/json", typeof(SignupRequest), Description = "Signup request", Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Signup success")]
        [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid email format, account already exists, or password does not meet minimum requirements")]
        [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Internal error, try again later.")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/signup")] HttpRequest req)
        {
            // TODO: Return body with error messages
            // TODO: Send verification email and implement Verify_v1

            var request = await req.ReadJson<SignupRequest>();

            if(request == null)
                return new BadRequestResult();

            if(!(Validation.IsValidEmail(request.Credentials?.Email) && Validation.IsValidPassword(request.Credentials?.Password)))
                return new BadRequestResult();

            var hash = Crypto.SHA256Base16(request.Credentials!.Email!);
            var query = await _storage.QueryAsync<UserEntity>(x => x.EmailHash == hash);

            await foreach(UserEntity ent in query)
            {
                return new BadRequestResult();
            }

            var uuid = Guid.NewGuid().ToString();
            var pw = request.Credentials!.Password!;
            
            var user = new UserEntity() { 
                UUID = uuid,
                EmailHash = hash
            };
            user.GenerateEncryptionKey(uuid, pw);
            user.ChangePassword(null, pw);

            var key = user.GetEncryptionKey(pw);
            var userData = new User {
                Email = request.Credentials.Email
            };
            user.SetPrivateUserData(userData, key!);

            bool result = await _storage.UpsertAsync(user);
            if(!result)
            {
                return new InternalServerErrorResult();
            }

            return new NoContentResult();
        }
    }
}

