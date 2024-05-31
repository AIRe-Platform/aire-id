using System.Net;
using System.Web.Http;
using Aire.Id.Models;
using Aire.Sdk.AspNetCore;
using Aire.Sdk.Auth;
using Aire.Sdk.Azure;
using Aire.Sdk.Helpers;
using Aire.Sdk.Models.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Aire.Id.Api.Admin;

public class Account_v1
{
    private readonly IJwtTokenService _jwt;
    private readonly ITableStorageService _storage;
    private readonly ILogger<Account_v1> _log;

    public Account_v1(IJwtTokenService jwt, ITableStorageService storage, ILogger<Account_v1> log)
    {
        _jwt = jwt;
        _storage = storage;
        _log = log;
    }

    [Function("GetAccountById_v1")]
    [OpenApiOperation(
        operationId: "getAccountById",
        tags: ["Admin", "Accounts"],
        Summary = "Get user account by ID",
        Description = "Finds user account by ID")]
    [OpenApiParameter("id", In = ParameterLocation.Path, Required = true, Description = "Account identifier")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Account), Description = "The account object")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or insufficient authorization")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid account ID format")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "The account does not exist")]
    public async Task<IActionResult> GetAccountById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/admin/account/{id}")] HttpRequest req,
            FunctionContext context,
            string id)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.AdminAccounts))
            return new ForbiddenResult();

        if (string.IsNullOrWhiteSpace(id))
            return new BadRequestResult();

        var entity = await _storage.RetrieveAsync<UserEntity>(id);
        if (entity == null)
        {
            _log.LogWarning("Account not found");
            return new NotFoundResult();
        }

        return new OkObjectResult(entity.ToAccountModel());
    }

    [Function("FindAccount_v1")]
    [OpenApiOperation(
        operationId: "findAccount",
        tags: ["Admin", "Accounts"],
        Summary = "Find account",
        Description = "Find account by using email or usernaname")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiParameter("login_name", In = ParameterLocation.Query, Description = "Username or email address")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Account), Description = "The account object")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or insufficient authorization")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid query")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "The account does not exist")]
    public async Task<IActionResult> FindAccount(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/admin/account")] HttpRequest req,
            FunctionContext context,
            [FromQuery] string login_name)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.AdminAccounts))
            return new ForbiddenResult();

        var hash = Crypto.SHA256Base16(login_name);
        var query = await _storage.QueryAsync<UserEntity>(x => x.EmailHash == hash || x.Username == login_name);
        var entity = await query.FirstOrDefaultAsync();

        if (entity == null)
        {
            _log.LogWarning("Account not found");
            return new NotFoundResult();
        }

        return new OkObjectResult(entity.ToAccountModel());
    }

    [Function("EditAccount_v1")]
    [OpenApiOperation(
        operationId: "editAccount",
        tags: ["Admin", "Accounts"],
        Summary = "Edit user account",
        Description = "Edit user account")]
    [OpenApiParameter("id", In = ParameterLocation.Path, Required = true, Description = "Account identifier")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiRequestBody("application/json", typeof(Account), Required = true, Description = "User account object")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Account), Description = "The saved account object")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or insufficient authorization")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid param or body")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "The account does not exist")]
    public async Task<IActionResult> EditAccount(
            [HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route = "api/v1/admin/account/{id}")] HttpRequest req,
            FunctionContext context,
            string id)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.AdminAccounts))
            return new ForbiddenResult();

        if (string.IsNullOrWhiteSpace(id))
            return new BadRequestResult();

        var data = await req.ReadJson<Account>();
        if (data == null)
        {
            _log.LogWarning("Failed to parse body");
            return new BadRequestResult();
        }

        var entity = await _storage.RetrieveAsync<UserEntity>(id);
        if (entity == null)
        {
            _log.LogWarning("Account not found");
            return new NotFoundResult();
        }

        if (data.Verified.HasValue)
            entity.Verified = data.Verified.Value;

        if (data.Role != null)
            entity.Role = data.Role;

        if (data.AdditionalScopes != null)
            entity.AdditionalScopes = string.Join(" ", data.AdditionalScopes);

        if (data.PublicName != null)
            entity.PublicName = data.PublicName;

        if (data.OverrideScopes)
        {
            if (data.Scopes == null)
                return new BadRequestResult();

            entity.Scopes = string.Join(" ", data.Scopes);
        }
        else
        {
            entity.Scopes = "";
        }

        var edit = await _storage.UpsertAsync(entity);
        if (!edit)
            return new InternalServerErrorResult();

        return new OkObjectResult(entity.ToAccountModel());
    }
}
