// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using System.Net;
using Aire.Sdk.AspNetCore;
using Aire.Sdk.Audit;
using Aire.Sdk.Auth;
using Aire.Sdk.Models.Audit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Aire.Id.Api;

public class Audit_v1(IJwtTokenService jwt, IAireAuditService auditService, ILogger<Audit_v1> log)
{
    [Function("GetAuditLog")]
    [OpenApiOperation(
        operationId: "getAuditLog",
        tags: ["Audit"],
        Summary = "Retrieve audit event log")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiParameter("resource", In = ParameterLocation.Query, Description = "Resource filter", Required = false)]
    [OpenApiParameter("from", In = ParameterLocation.Query, Description = "Retrieve audit events starting from", Required = false)]
    [OpenApiParameter("to", In = ParameterLocation.Query, Description = "Retrieve audit events ending to", Required = false)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AuditLog), Description = "Audit log")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Authorization required")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid query")]
    public async Task<IActionResult> GetAuditLog(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/audit")] HttpRequest req,
        FunctionContext context,
        [FromQuery] string? resource,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!jwt.CheckAuthorization(auth, requiredScopes: AireScopes.AdminAudit))
            return new ForbiddenResult();

        log.LogInformation("Retrieving audit log for resource '{0}'", resource ?? "*");

        if (from.HasValue)
            log.LogInformation("Starting from {0}", from.Value);

        if (to.HasValue)
            log.LogInformation("Ending to {0}", to.Value);

        try
        {
            AireAuditResource? res = null;

            if (!string.IsNullOrWhiteSpace(resource))
            {
                res = AireAuditResource.Parse(resource);
                if (res == null)
                    return new BadRequestResult();
            }

            var auditLog = await auditService.GetAuditLog(res, from, to);
            return new OkObjectResult(auditLog);
        }
        catch (ArgumentException)
        {
            return new BadRequestResult();
        }
        catch (Exception)
        {
            throw;
        }
    }
}