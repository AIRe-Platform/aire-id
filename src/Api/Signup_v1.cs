using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Aire.Helpers;
using Aire.Id.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

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

        [FunctionName("Signup_v1")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "User sign-up and verification" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Description = "Signup success")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Description = "Invalid email format, account already exists, or password does not meet minimum requirements")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/signup")] HttpRequest req)
        {
            // TODO: Return body with error messages
            // TODO: Send verification email and implement Verify_v1

            var request = await req.ReadJson<SignupRequest>();

            if(request == null)
                return new BadRequestResult();

            if(!(Validation.IsValidEmail(request.Credentials.Email) && Validation.IsValidPassword(request.Credentials.Password)))
                return new BadRequestResult();

            var hash = Crypto.SHA256Base16(request.Credentials.Email);
            var query = await _storage.QueryAsync<UserEntity>(x => x.EmailHash == hash);

            await foreach(UserEntity ent in query)
            {
                return new BadRequestResult();
            }

            var uuid = Guid.NewGuid().ToString();
            var user = new UserEntity() { 
                UUID = uuid,
                EmailHash = hash
            };
            user.ChangePassword(null, request.Credentials.Password);

            var key = Crypto.DeriveUserEncryptionKey(uuid, request.Credentials.Password);
            var userData = new User {
                Email = request.Credentials.Email
            };
            user.SetUserData(userData, key);

            bool result = await _storage.UpsertAsync(user);
            if(!result)
            {
                return new InternalServerErrorResult();
            }

            return new NoContentResult();
        }
    }
}

