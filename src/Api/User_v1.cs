using System.Net;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Aire.Id.Models;
using Aire.Id.Helpers;
using Aire.Sdk.AspNetCore;
using Aire.Sdk.Azure;
using Aire.Sdk.Auth;
using Aire.Sdk.Models.Identity;
using Aire.Sdk.Platform.Clients;

namespace Aire.Id.Api;

public class User_v1
{
    private readonly IJwtTokenService _jwt;
    private readonly ITableStorageService _storage;
    private readonly IAireClientFactory _clientFactory;
    private readonly ILogger<User_v1> _log;

    public User_v1(IJwtTokenService jwt, ITableStorageService storage, IAireClientFactory clientFactory, ILogger<User_v1> log)
    {
        _jwt = jwt;
        _storage = storage;
        _clientFactory = clientFactory;
        _log = log;
    }

    [Function("GetUser_v1")]
    [OpenApiOperation(
        operationId: "getUser",
        tags: ["User"],
        Summary = "Get user",
        Description = "Gets user identified by the token")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(User), Description = "The user object")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or insufficient authorization")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Not allowed to access the resource")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "The user does not exist")]
    public async Task<IActionResult> GetUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/user")] HttpRequest req,
        FunctionContext context)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!auth.VerifiedAccount)
        {
            var unverifiedUser = new User
            {
                UUID = auth.UserId,
                Verified = false
            };
            return new OkObjectResult(unverifiedUser);
        }

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.ReadProfile))
            return new ForbiddenResult();

        string id = auth!.Token.Subject;

        var userAccount = await _storage.RetrieveAsync<UserEntity>(id);
        if (userAccount != null)
        {
            _log.LogInformation("Account found");

            var user = userAccount.GetUserData(auth!.UserKey);
            if (user == null)
            {
                _log.LogWarning("Could not decrypt user data");
                return new ForbiddenResult();
            }

            if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.ConnectProfile))
                user.ConnectedServices = null;

            return new OkObjectResult(user);
        }

        return new NotFoundResult();
    }

    [Function("EditUser_v1")]
    [OpenApiOperation(
        operationId: "editUser",
        tags: ["User"],
        Summary = "Edit user",
        Description = "Edits user private data")]
    [OpenApiParameter("id", Description = "User identifier", In = ParameterLocation.Path, Required = true)]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiRequestBody("application/json", typeof(UserPrivate), Description = "User data", Required = true)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UserPrivate), Description = "The updated user object")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or insufficient authorization")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Not allowed to access the resource")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Parsing the data failed")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "The user does not exist")]
    [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Failed to save changes")]
    public async Task<IActionResult> EditUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/v1/user/{id}")] HttpRequest req,
        FunctionContext context,
        string id)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.EditProfile))
            return new ForbiddenResult();

        if (auth.UserId != id)
        {
            _log.LogWarning("Not allowed to edit other user's account");
            return new ForbiddenResult();
        }

        var entity = await _storage.RetrieveAsync<UserEntity>(id);
        if (entity == null)
        {
            _log.LogWarning("User entity not found");
            return new NotFoundResult();
        }

        var user = entity.GetPrivateUserData(auth!.UserKey);
        if (user == null)
        {
            _log.LogWarning("Failed to decrypt user data");
            return new UnauthorizedResult();
        }

        var userData = await req.ReadJson<UserPrivate>();
        if (userData == null)
        {
            _log.LogWarning("Failed to parse user data");
            return new BadRequestResult();
        }

        // Read-only fields
        {
            userData.Email = user.Email;
        }

        bool allowConnect = _jwt.CheckAuthorization(auth, AireScopes.ConnectProfile);
        if (!allowConnect)
            userData.ConnectedServices = user.ConnectedServices;

        entity.SetPrivateUserData(userData, auth!.UserKey);

        bool result = await _storage.UpsertAsync(entity);
        if (result)
        {
            var updated = entity.GetUserData(auth!.UserKey);

            if(!allowConnect)
                updated.ConnectedServices = null;

            return new OkObjectResult(updated);
        }
        else
        {
            _log.LogWarning("Failed to update user data");
            return new InternalServerErrorResult();
        }
    }

    [Function("ChangePassword_v1")]
    [OpenApiOperation(
        operationId: "changePassword",
        tags: ["User"],
        Summary = "Change user password")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiParameter("id", Description = "User identifier", In = ParameterLocation.Path, Required = true)]
    [OpenApiRequestBody("application/json", typeof(PasswordChangeRequest), Description = "Password change request", Required = true)]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "The password was changed successfully")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or insufficient authorization")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Not allowed to access the resource")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Failed to parse the request or the password does not meet the minimum requirements")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "The user does not exist")]
    [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Failed to save changes")]
    public async Task<IActionResult> ChangePassword(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/user/{id}/password")] HttpRequest req,
        FunctionContext context,
        string id)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.PasswordChange))
            return new ForbiddenResult();

        if (auth!.Token.Subject != id)
        {
            _log.LogWarning("Not allowed to edit other user's account");
            return new ForbiddenResult();
        }

        var body = await req.ReadJson<PasswordChangeRequest>();
        if (body == null)
        {
            _log.LogWarning("Could not parse request");
            return new BadRequestResult();
        }

        bool valid = Validation.IsValidPassword(body.NewPassword);
        if (!valid)
        {
            _log.LogWarning("New password is not valid");
            return new BadRequestResult();
        }

        var entity = await _storage.RetrieveAsync<UserEntity>(id);
        if (entity == null)
        {
            _log.LogWarning("User entity not found");
            return new NotFoundResult();
        }

        if (!entity.ChangePassword(body.CurrentPassword, body.NewPassword!))
        {
            _log.LogWarning("Failed to change password");
            return new BadRequestResult();
        }

        bool result = await _storage.UpsertAsync(entity);
        if (result)
        {
            return new NoContentResult();
        }
        else
        {
            _log.LogWarning("Failed to update user data");
            return new InternalServerErrorResult();
        }
    }

    [Function("DeleteUser_v1")]
    [OpenApiOperation(
        operationId: "deleteUser",
        tags: ["User"],
        Summary = "Delete user")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiParameter("id", Description = "User identifier", In = ParameterLocation.Path, Required = true)]
    [OpenApiRequestBody("application/json", typeof(UserDeleteRequest), Description = "Deletetion request", Required = true)]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Operation completed successfully")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or insufficient authorization")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Not allowed to access the resource")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Password confirmation failed")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "The user does not exist")]
    [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Failed to save changes")]
    public async Task<IActionResult> DeleteUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/v1/user/{id}")] HttpRequest req,
        FunctionContext context,
        string id)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.DeleteProfile))
            return new ForbiddenResult();

        if (auth!.Token.Subject != id)
        {
            _log.LogWarning("Not allowed to edit other user's account");
            return new ForbiddenResult();
        }

        var user = await _storage.RetrieveAsync<UserEntity>(id);
        if (user == null)
        {
            _log.LogWarning("User entity not found");
            return new NotFoundResult();
        }

        var options = await req.ReadJson<UserDeleteRequest>();
        if (options == null || string.IsNullOrWhiteSpace(options.Password))
        {
            _log.LogWarning("Password confirmation required!");
            return new BadRequestResult();
        }

        if (!user.CheckPassword(options.Password))
        {
            _log.LogWarning("Incorrect password confirmation");
            return new BadRequestResult();
        }

        // Delete user data from Memory module
        var memoryService = await _clientFactory.CreateMemoryClient(auth!.JwtEncodedToken);
        if (memoryService != null)
        {
            bool dataDeleted = await memoryService.DeleteUserData(options.KeepAnonymizedData);
            if (!dataDeleted)
            {
                _log.LogCritical("Failure to delete data from the Memory module");
                return new InternalServerErrorResult();
            }
        }

        // Delete user when data is destroyed
        bool userDeleted = await _storage.DeleteAsync(user);
        if (userDeleted)
        {
            return new NoContentResult();
        }
        else
        {
            _log.LogWarning("Failed to delete the user");
            return new InternalServerErrorResult();
        }
    }
}

