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
using System.Web.Http;

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
            [JwtToken(AllowRoles = "user", RequireScopes = "profile-read")] JwtSecurityToken token,
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
            [JwtToken(AllowRoles = "user", RequireScopes = "profile-edit")] JwtSecurityToken token,
            string id)
        {
            if(token == null)
                return new UnauthorizedResult();

            if(token.Subject != id)
            {
                _log.LogWarning("Not allowed to edit other user's account");
                return new ForbiddenResult();
            }

            var userKey = token.Claims.FirstOrDefault(x => x.Type == "user_enc_key")?.Value;
            if(string.IsNullOrWhiteSpace(userKey))
            {
                _log.LogWarning("Missing user encryption key");
                return new ForbiddenResult();
            }

            var entity = await _storage.RetrieveAsync<UserEntity>(id);
            if(entity == null)
            {
                _log.LogWarning("User entity not found");
                return new NotFoundResult();
            }

            var user = entity.GetPrivateUserData(userKey);
            if(user == null)
            {
                _log.LogWarning("Failed to decrypt user data");
                return new UnauthorizedResult();
            }

            var userData = await req.ReadJson<UserPrivate>();
            if(userData == null)
            {
                _log.LogWarning("Failed to parse user data");
                return new BadRequestResult();
            }

            // Read-only fields
            {
                userData.Email = user.Email;
                userData.ConnectedServices = user.ConnectedServices;
            }
            entity.SetPrivateUserData(userData, userKey);


            bool result = await _storage.UpsertAsync(entity);
            if(result)
            {
                var updated = entity.GetUserData(userKey);
                return new OkObjectResult(updated);
            }
            else
            {
                _log.LogWarning("Failed to update user data");
                return new InternalServerErrorResult();
            }
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
            [JwtToken(AllowRoles = "user", RequireScopes = "profile-delete")] JwtSecurityToken token,
            string id)
        {
            if(token == null)
                return new UnauthorizedResult();

            if(token.Subject != id)
            {
                _log.LogWarning("Not allowed to edit other user's account");
                return new ForbiddenResult();
            }
            
            var user = await _storage.RetrieveAsync<UserEntity>(id);
            if(user == null)
            {
                _log.LogWarning("User entity not found");
                return new NotFoundResult();
            }

            var options = await req.ReadJson<UserDeleteRequest>();
            if(options == null || string.IsNullOrWhiteSpace(options.Password))
            {
                _log.LogWarning("Password confirmation required!");
                return new BadRequestResult();
            }

            if(!user.CheckPassword(options.Password))
            {
                _log.LogWarning("Incorrect password confirmation");
                return new BadRequestResult();
            }

            bool result = await _storage.DeleteAsync(user);
            if(result)
            {
                if(!options.KeepAnonymizedData)
                {
                    _log.LogWarning("REMOVING PLATFORM-WIDE USER DATA IS NOT IMPLEMENTED YET");
                }
                return new NoContentResult();
            }
            else
            {
                _log.LogWarning("Failed to delete the user");
                return new InternalServerErrorResult();
            }
        }
    }
}

