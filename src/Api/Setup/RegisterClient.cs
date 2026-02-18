// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Aire.Sdk.Azure;
using Aire.Sdk.AspNetCore;
using Aire.Id.Models;
using Aire.Sdk.Models.Admin;
using Newtonsoft.Json;
using Aire.Sdk.Helpers;
using System.Security.Cryptography;
using Azure;

namespace Aire.Id.Api.Setup;

public class RegisterClient
{
    private readonly ILogger<RegisterClient> _log;
    private readonly ITableStorageService _storage;

    public RegisterClient(ILogger<RegisterClient> log, ITableStorageService storage)
    {
        _log = log;
        _storage = storage;
    }

    internal class RegistrationResult
    {
        [JsonProperty("client_id")]
        public string? ClientId { get; set; }

        [JsonProperty("client_secret", NullValueHandling = NullValueHandling.Include)]
        public string? ClientSecret { get; set; }
    }

    [Function("RegisterClient")]
    [OpenApiIgnore]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Admin, "post", Route = "api/setup/client")] HttpRequest req)
    {
        var client = await req.ReadJson<Client>();

        if (client == null)
            return new UnprocessableEntityResult();

        Uri.TryCreate(client.RedirectUri, UriKind.Absolute, out Uri? redirect);
        if (redirect == null)
            return new UnprocessableEntityResult();

        var entity = new ClientEntity()
        {
            Name = client.Name,
            Active = client.Active ?? false,
            RedirectUri = redirect.AbsoluteUri,
            Public = client.Public ?? true,
            RequireConsent = client.RequireConsent ?? true,
            RequirePlatform = client.RequirePlatform ?? true
        };

        entity.SetAllowedScopes(client.Scopes ?? ["*"]);
        entity.SetGrantTypes(client.GrantTypes ?? ["authorization_code"]);
        entity.SetAllowedPlatforms(client.Platforms ?? []);

        string? clientSecret = null;
        if (client.Public == false)
        {
            clientSecret = RandomNumberGenerator.GetHexString(32, true);
            entity.SecretHash = Crypto.SHA256Base64(clientSecret);
        }

        var insert = await _storage.UpsertAsync(entity);
        if (!insert)
            throw new RequestFailedException("Failed to insert entity");

        var result = new RegistrationResult()
        {
            ClientId = entity.Id(),
            ClientSecret = clientSecret
        };

        return new OkObjectResult(result);
    }
}
