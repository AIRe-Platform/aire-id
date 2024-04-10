using System.Net;
using System.Web.Http;
using Aire.Sdk.AspNetCore;
using Aire.Sdk.Auth;
using Aire.Sdk.Azure;
using Aire.Sdk.Models.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Aire.Id.Api.Admin;

public class Client_v1
{
    private readonly IJwtTokenService _jwt;
    private readonly ITableStorageService _storage;
    private readonly ILogger<Client_v1> _log;

    public Client_v1(IJwtTokenService jwt, ITableStorageService storage, ILogger<Client_v1> log)
    {
        _jwt = jwt;
        _storage = storage;
        _log = log;
    }

    [Function("GetClientById_v1")]
    [OpenApiOperation(
            operationId: "getClientById",
            tags: ["Admin", "Clients"],
            Summary = "Get a client by its ID",
            Description = "Finds an OAuth 2.0 client by the its ID")]
    [OpenApiParameter("id", In = ParameterLocation.Path, Required = true, Description = "Client identifier")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Client), Description = "The client object")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or insufficient authorization")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid client id")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "The client does not exist")]
    public async Task<IActionResult> GetClientById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/admin/client/{id}")] HttpRequest req,
        FunctionContext context,
        [FromRoute] string id)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, AireScopes.AdminClients))
            return new ForbiddenResult();

        if (string.IsNullOrWhiteSpace(id))
            return new BadRequestResult();

        var client = await _storage.RetrieveAsync<ClientEntity>(id);
        if (client == null)
            return new NotFoundResult();

        return new OkObjectResult(client.ToModel());
    }

    [Function("GetClients_v1")]
    [OpenApiOperation(
        operationId: "getClients",
        tags: ["Admin", "Clients"],
        Summary = "List clients",
        Description = "List all OAuth 2.0 clients")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<Client>), Description = "A list of clients")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or insufficient authorization")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    public async Task<IActionResult> GetClients(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/admin/clients")] HttpRequest req,
        FunctionContext context)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, AireScopes.AdminClients))
            return new ForbiddenResult();

        var entities = await _storage.All<ClientEntity>();
        var clients = entities.Select(x => x.ToModel()).ToList();

        return new OkObjectResult(clients);
    }

    [Function("CreateClient_v1")]
    [OpenApiOperation(
        operationId: "createClient",
        tags: ["Admin", "Clients"],
        Summary = "Register a client",
        Description = "Registers a new OAuth 2.0 client")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiRequestBody("application/json", typeof(Client), Required = true, Description = "Client model")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Client), Description = "The client object")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or insufficient authorization")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid body")]
    public async Task<IActionResult> CreateClient(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/admin/client")] HttpRequest req,
        FunctionContext context)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, AireScopes.AdminClients))
            return new ForbiddenResult();

        var client = await req.ReadJson<Client>();
        if (client == null)
            return new BadRequestResult();

        if (string.IsNullOrWhiteSpace(client.Name) || client.Scopes == null ||
            !Uri.TryCreate(client.RedirectUri, UriKind.Absolute, out Uri? redirectUri))
        {
            return new BadRequestResult();
        }

        var entity = new ClientEntity()
        {
            Name = client.Name,
            Active = client.Active ?? false,
            AllowedScopes = string.Join(" ", client.Scopes),
            RedirectUri = redirectUri.AbsoluteUri
        };

        var insert = await _storage.UpsertAsync(entity);
        if (!insert)
            return new InternalServerErrorResult();

        return new OkObjectResult(entity.ToModel());
    }

    [Function("EditClient_v1")]
    [OpenApiOperation(
        operationId: "editClient",
        tags: ["Admin", "Clients"],
        Summary = "Edit a client",
        Description = "Edit an OAuth 2.0 client")]
    [OpenApiParameter("id", In = ParameterLocation.Path, Required = true, Description = "Client identifier")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiRequestBody("application/json", typeof(Client), Required = true, Description = "Client model")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Client), Description = "The client object")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or insufficient authorization")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid body or ID")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "The client was not found")]
    public async Task<IActionResult> EditClient(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/admin/client/{id}")] HttpRequest req,
        FunctionContext context,
        [FromRoute] string id)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, AireScopes.AdminClients))
            return new ForbiddenResult();

        if (string.IsNullOrWhiteSpace(id))
            return new BadRequestResult();

        var data = await req.ReadJson<Client>();
        if (data == null)
            return new BadRequestResult();

        if (data.Id != id)
            return new BadRequestResult();

        var client = await _storage.RetrieveAsync<ClientEntity>(id);
        if (client == null)
            return new NotFoundResult();

        if (data.Scopes != null)
            client.AllowedScopes = string.Join(" ", data.Scopes);

        if (data.Name != null)
            client.Name = data.Name;

        if (data.Active.HasValue)
            client.Active = data.Active.Value;

        if (data.RedirectUri != null)
        {
            if (!Uri.TryCreate(data.RedirectUri, UriKind.Absolute, out Uri? uri))
                return new BadRequestResult();

            client.RedirectUri = uri.AbsoluteUri;
        }

        var update = await _storage.UpsertAsync(client);
        if (!update)
            return new InternalServerErrorResult();

        return new OkObjectResult(client.ToModel());
    }

    [Function("DeleteClient_v1")]
    [OpenApiOperation(
        operationId: "deleteClient",
        tags: ["Admin", "Clients"],
        Summary = "Delete a client",
        Description = "Delete an OAuth 2.0 client")]
    [OpenApiParameter("id", In = ParameterLocation.Path, Required = true, Description = "Client identifier")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "The client was deleted successfully")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Missing or insufficient authorization")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid id")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "The client was not found")]
    public async Task<IActionResult> DeleteClient(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/admin/client/{id}")] HttpRequest req,
        FunctionContext context,
        [FromRoute] string id)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, AireScopes.AdminClients))
            return new ForbiddenResult();

        if (string.IsNullOrWhiteSpace(id))
            return new BadRequestResult();

        var client = await _storage.RetrieveAsync<ClientEntity>(id);
        if (client == null)
            return new NotFoundResult();

        var delete = await _storage.DeleteAsync(client);
        if (!delete)
            return new InternalServerErrorResult();

        return new NoContentResult();
    }
}
