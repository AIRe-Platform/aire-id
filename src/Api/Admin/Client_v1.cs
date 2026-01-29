// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using System.Net;
using System.Security.Cryptography;
using Aire.Id.Models;
using Aire.Sdk.AspNetCore;
using Aire.Sdk.Auth;
using Aire.Sdk.Azure;
using Aire.Sdk.Helpers;
using Aire.Sdk.Models.Admin;
using Azure;
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
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/admin/client/{id}")] HttpRequest req,
        FunctionContext context,
        [FromRoute] string id)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, AireScopes.ReadClients))
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
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/admin/clients")] HttpRequest req,
        FunctionContext context)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, AireScopes.ReadClients))
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
    [OpenApiResponseWithoutBody(HttpStatusCode.Conflict, Description = "A client with the same name already exists")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid body")]
    public async Task<IActionResult> CreateClient(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/admin/client")] HttpRequest req,
        FunctionContext context)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, AireScopes.CreateClients))
            return new ForbiddenResult();

        var client = await req.ReadJson<Client>();
        if (client == null)
            return new BadRequestResult();

        if (string.IsNullOrWhiteSpace(client.Name) || client.Scopes == null ||
            !Uri.TryCreate(client.RedirectUri, UriKind.Absolute, out Uri? redirectUri))
        {
            return new BadRequestResult();
        }

        var existing_query = await _storage.QueryAsync<ClientEntity>(x => x.Name == client.Name);
        var existing_entity = await existing_query.FirstOrDefaultAsync();

        if (existing_entity != null)
        {
            return new ConflictResult();
        }

        var entity = new ClientEntity()
        {
            Name = client.Name,
            Active = client.Active ?? false,
            RedirectUri = redirectUri.AbsoluteUri,
            Public = client.Public ?? true,
            RequireConsent = client.RequireConsent ?? true,
            RequirePlatform = client.RequirePlatform ?? true,
        };

        entity.SetAllowedScopes(client.Scopes);
        entity.SetAllowedPlatforms(client.Platforms ?? []);
        entity.SetGrantTypes(client.GrantTypes ?? []);

        string? clientSecret = null;
        if (client.Public == false)
        {
            clientSecret = RandomNumberGenerator.GetHexString(32, true);
            entity.SecretHash = Crypto.SHA256Base64(clientSecret);
        }

        var insert = await _storage.UpsertAsync(entity);
        if (!insert)
            throw new RequestFailedException("Failed to insert entity");

        var model = entity.ToModel();
        model.Secret = clientSecret;

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
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/v1/admin/client/{id}")] HttpRequest req,
        FunctionContext context,
        [FromRoute] string id)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, AireScopes.EditClients))
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
            client.SetAllowedScopes(data.Scopes);

        if (data.Name != null)
        {
            client.Name = data.Name;

            var existing_query = await _storage.QueryAsync<ClientEntity>(x => x.Name == client.Name);
            var existing_entity = await existing_query.FirstOrDefaultAsync();

            if (existing_entity != null && existing_entity.Id() != client.Id())
            {
                return new ConflictResult();
            }
        }

        if (data.Active.HasValue)
            client.Active = data.Active.Value;

        if (data.RequireConsent.HasValue)
            client.RequireConsent = data.RequireConsent.Value;

        if (data.RedirectUri != null)
        {
            if (!Uri.TryCreate(data.RedirectUri, UriKind.Absolute, out Uri? uri))
                return new BadRequestResult();

            client.RedirectUri = uri.AbsoluteUri;
        }

        if (data.Public.HasValue)
            client.Public = data.Public.Value;

        if (data.GrantTypes != null)
            client.SetGrantTypes(data.GrantTypes);

        if (data.RequirePlatform.HasValue)
            client.RequirePlatform = data.RequirePlatform.Value;

        if (data.Platforms != null)
            client.SetAllowedPlatforms(data.Platforms);

        var update = await _storage.UpsertAsync(client);
        if (!update)
            throw new RequestFailedException("Failed to update entity");

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
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/v1/admin/client/{id}")] HttpRequest req,
        FunctionContext context,
        [FromRoute] string id)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, AireScopes.DeleteClients))
            return new ForbiddenResult();

        if (string.IsNullOrWhiteSpace(id))
            return new BadRequestResult();

        var client = await _storage.RetrieveAsync<ClientEntity>(id);
        if (client == null)
            return new NotFoundResult();

        var delete = await _storage.DeleteAsync(client);
        if (!delete)
            throw new RequestFailedException("Failed to delete entity");

        return new NoContentResult();
    }
}
