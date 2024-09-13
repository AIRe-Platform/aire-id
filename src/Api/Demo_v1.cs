// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Aire.Id.Models;
using Aire.Id.Oauth2.Providers;
using Aire.Sdk.AspNetCore;
using Aire.Sdk.Auth;
using Aire.Sdk.Azure;
using Aire.Sdk.Models.Identity;
using Aire.Sdk.Models.Demo;
using Aire.Sdk.Platform.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Aire.Id.Api;

public class Demo_v1
{
    private readonly IJwtTokenService _jwt;
    private readonly ITableStorageService _storage;
    private readonly IAireClientFactory _clientFactory;
    private readonly IOauthLoginProvider _loginProvider;
    private readonly IOauthTokenProvider _tokenProvider;
    private readonly ILogger<Demo_v1> _log;

    public Demo_v1(
        IJwtTokenService jwt, ITableStorageService storage, IAireClientFactory clientFactory,
        IOauthLoginProvider loginProvider, IOauthTokenProvider tokenProvider, ILogger<Demo_v1> log)
    {
        _jwt = jwt;
        _storage = storage;
        _clientFactory = clientFactory;
        _loginProvider = loginProvider;
        _tokenProvider = tokenProvider;
        _log = log;
    }

    [Function("GetDemoGroups_v1")]
    [OpenApiOperation(
        operationId: "getDemoGroups",
        tags: ["Demo"],
        Summary = "Get list of demo groups")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<DemoGroup>), Description = "List of demo groups")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Authorization required")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Internal error")]
    public async Task<IActionResult> GetDemoGroups(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/demo/groups")] HttpRequest req,
        FunctionContext context)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.ReadDemoGroups))
            return new ForbiddenResult();

        var query = await _storage.QueryAsync<DemoGroupEntity>(_ => true);
        var groups = await query.Select(x => x.ToModel()).ToListAsync();

        return new OkObjectResult(groups);
    }

    [Function("GetDemoUsers_v1")]
    [OpenApiOperation(
        operationId: "getDemoUsers",
        tags: ["Demo"],
        Summary = "Get list of demo users in a group")]
    [OpenApiParameter("id", Description = "Group identifier", In = ParameterLocation.Path)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<DemoUser>), Description = "List of demo users")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Authotization required")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid param")]
    [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Internal error")]
    public async Task<IActionResult> GetDemoUsers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/demo/group/{id}")] HttpRequest req,
        FunctionContext context,
        string id)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.ReadDemoGroups))
            return new ForbiddenResult();

        if (string.IsNullOrWhiteSpace(id))
            return new BadRequestResult();

        var group = await _storage.RetrieveAsync<DemoGroupEntity>(id);
        if (group == null)
            return new NotFoundResult();

        return new OkObjectResult(group.Users);
    }

    [Function("GetDemoUser_v1")]
    [OpenApiOperation(
        operationId: "getDemoUser",
        tags: ["Demo"],
        Summary = "Get demo user")]
    [OpenApiParameter("id", Description = "User identifier", In = ParameterLocation.Path)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(User), Description = "Demo user profile")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Authorization required")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid param")]
    [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Internal error")]
    public async Task<IActionResult> GetDemoUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/demo/user/{id}")] HttpRequest req,
        FunctionContext context,
        string id)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.ReadDemoGroups))
            return new ForbiddenResult();

        if (string.IsNullOrWhiteSpace(id))
            return new BadRequestResult();

        var user = await _storage.RetrieveAsync<DemoUserEntity>(id);
        if (user == null || user.Role != AireRoles.DemoUser)
            return new NotFoundResult();

        var key = user.GetEncryptionKey(user.DemoAccessCode!)!;
        return new OkObjectResult(user.GetUserData(key));
    }

    [Function("CreateDemoGroup_v1")]
    [OpenApiOperation(
        operationId: "createDemoGroup",
        tags: ["Demo"],
        Summary = "Create a demo group")]
    [OpenApiRequestBody("application/json", typeof(DemoGroupCreateRequest), Description = "Group information")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DemoGroup), Description = "Demo group object")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Authorization required")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid request")]
    [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Internal error")]
    public async Task<IActionResult> CreateDemoGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/demo/group")] HttpRequest req,
        FunctionContext context)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.EditDemoGroups))
            return new ForbiddenResult();

        var group = await req.ReadJson<DemoGroupCreateRequest>();
        if (group == null)
            return new BadRequestResult();

        if (string.IsNullOrWhiteSpace(group.Name) ||
            string.IsNullOrWhiteSpace(group.UsernamePrefix) ||
            !Regex.IsMatch(group.UsernamePrefix, @"^[^_-]([A-Za-z0-9]+[_-]?)+$") ||
            group.Count < 1 || group.Count > 100)
        {
            return new BadRequestResult();
        }

        var query = await _storage
            .QueryAsync<DemoGroupEntity>(x => x.Name == group.Name || x.UsernamePrefix == group.UsernamePrefix);
        var existing = await query.FirstOrDefaultAsync();
        if (existing != null)
            return new ConflictResult();

        List<DemoUser> subjects = [];
        var groupId = Guid.NewGuid().ToString();

        for (int i = 0; i < group.Count!; i++)
        {
            var accessCode = RandomNumberGenerator.GetHexString(8, true);
            var entity = new DemoUserEntity()
            {
                Role = AireRoles.DemoUser,
                Username = $"{group.UsernamePrefix!}{i}",
                DemoGroupId = groupId,
                DemoAccessCode = accessCode,
                Verified = true
            };
            entity.GenerateEncryptionKey(accessCode);
            entity.ChangePassword(null, accessCode);

            var key = entity.GetEncryptionKey(accessCode)!;
            entity.SetPrivateUserData(new UserPrivate(), key);

            bool result = await _storage.UpsertAsync(entity);
            if (!result)
                throw new SystemException("Failed to create demo user");

            subjects.Add(new DemoUser
            {
                Id = entity.UUID(),
                GroupId = groupId,
                Username = entity.Username,
                AccessCode = accessCode
            });
        }

        var groupEntity = new DemoGroupEntity(groupId)
        {
            Name = group.Name,
            UsernamePrefix = group.UsernamePrefix,
            Users = subjects,
            Active = true
        };

        if (!await _storage.UpsertAsync(groupEntity))
            throw new SystemException("Failed to insert group entity");

        return new OkObjectResult(groupEntity.ToModel());
    }

    [Function("EditDemoGroup_v1")]
    [OpenApiOperation(
        operationId: "editDemoGroup",
        tags: ["Demo"],
        Summary = "Edit a demo group")]
    [OpenApiParameter("id", Description = "Group identifier", In = ParameterLocation.Path)]
    [OpenApiRequestBody("application/json", typeof(DemoGroup), Description = "Research group")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DemoGroup), Description = "Edited demo group")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "The group does not exist")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or insufficient authorization")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid request")]
    [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Internal error")]
    public async Task<IActionResult> EditDemoGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/v1/demo/group/{id}")] HttpRequest req,
        FunctionContext context,
        string id)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.EditDemoGroups))
            return new ForbiddenResult();

        var group = await req.ReadJson<DemoGroup>();
        if (group == null)
            return new BadRequestResult();

        if (string.IsNullOrWhiteSpace(group.Name))
            return new BadRequestResult();

        if (string.IsNullOrWhiteSpace(id))
            return new BadRequestResult();

        var entity = await _storage.RetrieveAsync<DemoGroupEntity>(id);
        if (entity == null)
            return new NotFoundResult();

        if (group.Id != null && entity.RowKey != group.Id)
            return new BadRequestResult();

        if (!string.IsNullOrWhiteSpace(group.Name))
            entity.Name = group.Name;

        if (group.Active.HasValue)
            entity.Active = group.Active;

        if (!await _storage.UpsertAsync(entity))
            throw new SystemException("Failed to insert group entity");

        return new OkObjectResult(entity.ToModel());
    }

    [Function("DeleteDemoGroup_v1")]
    [OpenApiOperation(
        operationId: "deleteDemoGroup",
        tags: ["Demo"],
        Summary = "Delete a demo group")]
    [OpenApiParameter("id", Description = "Group identifier", In = ParameterLocation.Path)]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Operation completed succesfully")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or insufficient authorization")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid param")]
    [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Internal error")]
    public async Task<IActionResult> DeleteResearchGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/v1/demo/group/{id}")] HttpRequest req,
        FunctionContext context,
        string id)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.DeleteDemoGroups))
            return new ForbiddenResult();

        if (string.IsNullOrWhiteSpace(id))
            return new BadRequestResult();

        var group = await _storage.RetrieveAsync<DemoGroupEntity>(id);
        if (group == null)
            return new NotFoundResult();

        foreach (var subject in group.Users!)
        {
            var entity = await _storage.RetrieveAsync<UserEntity>(subject.Id!);
            if (entity == null)
            {
                _log.LogWarning($"Demo user '{subject.Id}' not found. Already deleted?");
                continue;
            }

            // Delete user data from Memory (pretend to be the subject)
            var subjectKey = entity.GetEncryptionKey(subject.AccessCode!);
            var oauthSubject = _loginProvider.GetSubject(entity, subjectKey!);
            var token = _tokenProvider.IssueNewToken(new Oauth2.Models.OauthTokenDescription
            {
                Subject = oauthSubject,
                Lifetime = TimeSpan.FromMinutes(5)
            });

            var memoryService = await _clientFactory.CreateMemoryClient(token);
            if (memoryService != null)
            {
                bool deleteData = await memoryService.DeleteUserData(true);
                if (!deleteData)
                    throw new Exception($"Failure to destroy user '{entity.RowKey}' data from Memory");
            }

            // Delete user entity
            if (entity != null)
            {
                bool deleteResult = await _storage.DeleteAsync(entity);
                if (!deleteResult)
                    throw new Exception($"Failure to delete demo user '{entity.RowKey}'");
            }
        }

        bool result = await _storage.DeleteAsync(group);
        if (!result)
            throw new Exception("Failed to delete demo group");

        return new NoContentResult();
    }
}
