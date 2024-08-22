// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Sdk.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Aire.Id.Api;

public class Logout_v1
{
    public Logout_v1()
    {

    }

    /// <summary>
    /// Redirect to frontend to remove authentication session
    /// </summary>
    [Function("GetLogout_v1")]
    [OpenApiIgnore]
    public static IActionResult GetLoginAuth(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/logout")] HttpRequest req,
        FunctionContext context)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        var query = req.Query.ToDictionary();
        var uri = QueryHelpers.AddQueryString(AireConstants.AppLogoutPath, query);

        return new RedirectResult(uri, false, false);
    }
}
