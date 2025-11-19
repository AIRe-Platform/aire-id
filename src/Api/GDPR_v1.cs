// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using System.Net;
using Aire.Id.Models;
using Aire.Sdk.Auth;
using Aire.Sdk.Azure;
using Aire.Sdk.Models;
using Aire.Sdk.Models.Platform;
using Aire.Sdk.Platform;
using Aire.Sdk.Platform.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace Aire.Id.Api;

public class GDPR_v1(
    IAireClientFactory clientFactory,
    ITableStorageService storage,
    IAirePlatformService platformService,
    IJwtTokenService jwt,
    ILogger<GDPR_v1> log)
{
    private readonly ILogger<GDPR_v1> _log = log;
    private readonly IAireClientFactory _clientFactory = clientFactory;
    private readonly ITableStorageService _storage = storage;
    private readonly IAirePlatformService _platformService = platformService;
    private readonly IJwtTokenService _jwt = jwt;

    [Function("GDPR_PersonalData_v1")]
    [OpenApiOperation("gdprPersonalData", ["GDPR"], Summary = "Get all personal data")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GDPRDataCollection), Description = "Personal data collection")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Authorization required")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Internal error")]
    public async Task<IActionResult> GDPR_PersonalData(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/gdpr/personal-data")] HttpRequest req,
        FunctionContext context)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        var platforms = await _platformService.GetPlatformConfigurations();
        var data = new GDPRDataCollection()
        {
            Memory = []
        };

        // Gather data from the memory services
        foreach (var platform in platforms)
        {
            var memories = platform.Value.GetModules(ModuleType.Memory, false);
            foreach (var memory in memories)
            {
                var memoryClient = await _clientFactory.CreateMemoryClient(platform.Key, auth.JwtEncodedToken, memory.Id);
                if (memoryClient != null)
                {
                    var userData = await memoryClient.GetUserData();
                    if (userData != null)
                    {
                        data.Memory.Add($"{platform.Key}/{memory.Id}", userData);
                    }
                }
            }
        }

        // Get user profile (require read-profile scope)
        if (_jwt.CheckAuthorization(auth, AireScopes.ReadProfile))
        {
            var user = await _storage.RetrieveAsync<UserEntity>(auth.UserId);
            if (user != null)
            {
                var profile = user.GetUserData(auth.UserKey);
                data.Profile = profile;
            }
        }

        return new OkObjectResult(data);
    }
}