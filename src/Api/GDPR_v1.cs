// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using System.Net;
using Aire.Id.Models;
using Aire.Sdk.Auth;
using Aire.Sdk.Azure;
using Aire.Sdk.Models;
using Aire.Sdk.Platform.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace Aire.Id.Api;

public class GDPR_v1
{
    private readonly ILogger<GDPR_v1> _log;
    private readonly IAireClientFactory _clientFactory;
    private readonly ITableStorageService _storage;
    private readonly IJwtTokenService _jwt;

    public GDPR_v1(
        ILogger<GDPR_v1> log,
        IAireClientFactory clientFactory,
        ITableStorageService storage,
        IJwtTokenService jwt)
    {
        _log = log;
        _clientFactory = clientFactory;
        _storage = storage;
        _jwt = jwt;
    }

    [Function("GDPR_PersonalData_v1")]
    [OpenApiOperation(
        operationId: "gdprPersonalData",
        tags: ["GDPR"],
        Summary = "Get all personal data")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Dictionary<string, object?>), Description = "Personal data collection")]
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

        var data = new GDPRDataCollection();

        // Gather data from the memory service
        var memoryClient = await _clientFactory.CreateMemoryClient(auth.JwtEncodedToken);
        if (memoryClient != null)
        {
            var userData = await memoryClient.GetUserData();
            if(userData != null)
            {
                data = userData;
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