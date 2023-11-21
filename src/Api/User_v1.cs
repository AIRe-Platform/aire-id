using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Threading.Tasks;
using Aire.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System;
using Aire.Id.Models;
using System.Linq;

namespace Aire.Id.Api
{
    public class User_v1
    {
        private readonly ITableStorageService _storage;
        private readonly ILogger<User_v1> _log;

        public User_v1(ITableStorageService storage, ILogger<User_v1> log)
        {
            _storage = storage;
            _log = log;
        }

        [FunctionName("User_v1_GET")]
        [OpenApiOperation(operationId: "Get User", tags: new[] { "User" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter("id", Description = "User identifier", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The user object")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Description = "Missing or invalid authorization header")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Forbidden, Description = "Not allowed to access the resource")]
        public async Task<IActionResult> GetUser(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/user/{id?}")] HttpRequest req,
            [JwtToken(AllowRoles = "user")] JwtSecurityToken token,
            string id)
        {
            if(token == null)
                return new UnauthorizedResult();

            if(!string.IsNullOrEmpty(id) && id != token.Subject)
            {
                _log.LogWarning("Not allowed to access other user's account");
                return new ForbiddenResult();
            }
            else
            {
                id = token.Subject;
            }

            var userAccount = await _storage.RetrieveAsync<UserEntity>(id);
            if(userAccount != null)
            {
                _log.LogInformation("Account found");
                
                var userKey = token.Claims.FirstOrDefault(x => x.Type == "user_enc_key")?.Value;
                if(string.IsNullOrWhiteSpace(userKey))
                {
                    _log.LogWarning("Missing user encryption key");
                    return new ForbiddenResult();
                }

                var user = userAccount.GetUserData(userKey);
                if(user == null)
                {
                    _log.LogWarning("Could not decrypt user data");
                    return new ForbiddenResult();
                }

                return new OkObjectResult(user);
            }

            return new NotFoundResult();
        }

        [FunctionName("User_v1_PUT")]
        [OpenApiOperation(operationId: "Edit User", tags: new[] { "User" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter("id", Description = "User identifier", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The updated user object")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Description = "Missing or invalid authorization header")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Forbidden, Description = "Not allowed to access the resource")]
        public async Task<IActionResult> EditUser(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "v1/user/{id}")] HttpRequest req,
            string id)
        {
            // TODO
            return await Task.FromResult(new NotFoundResult());
        }

        [FunctionName("User_v1_DELETE")]
        [OpenApiOperation(operationId: "Delete User", tags: new[] { "User" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter("id", Description = "User identifier", Required = true)]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Description = "Operation completed successfully")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Description = "Missing or invalid authorization header")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Forbidden, Description = "Not allowed to access the resource")]
        public async Task<IActionResult> DeleteUser(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "v1/user/{id}")] HttpRequest req,
            string id)
        {
            // TODO
            return await Task.FromResult(new NotFoundResult());
        }
    }
}

