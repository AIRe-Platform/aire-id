// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Aire.Id.Oauth2;
using Aire.Id.Oauth2.Models;
using Aire.Sdk.Auth;

namespace Aire.Id.Api
{
    public class Auth
    {
        private readonly OauthAuthenticationService _svc;
        private readonly IJwtTokenService _jwt;
        private readonly ILogger<Auth> _log;

        public Auth(OauthAuthenticationService svc, IJwtTokenService jwt, ILogger<Auth> log)
        {
            _svc = svc;
            _jwt = jwt;
            _log = log;
        }

        [Function("Oauth_Auth_Get")]
        [OpenApiOperation(
            operationId: "oauthAuthGet",
            tags: ["OAuth2"],
            Summary = "OAuth2 Auth Endpoint",
            Description = "OAuth2 authorization endpoint using GET")]
        [OpenApiParameter("response_type", Required = true, In = ParameterLocation.Query)]
        [OpenApiParameter("client_id", Required = true, In = ParameterLocation.Query)]
        [OpenApiParameter("redirect_uri", Required = false, In = ParameterLocation.Query)]
        [OpenApiParameter("scope", Required = false, In = ParameterLocation.Query)]
        [OpenApiParameter("state", Required = false, In = ParameterLocation.Query)]
        [OpenApiResponseWithoutBody(HttpStatusCode.Redirect, Description = "Redirect to the consent page or the redirect URI")]
        public async Task<IActionResult> Oauth_Authorize_Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/oauth/authorize")] HttpRequest req,
            FunctionContext context)
        {
            var auth = context.Features.Get<JwtAuthFeature>();
            return await _svc.HandleAuthRequest(req, auth);
        }

        [Function("Oauth_Auth_Post")]
        [OpenApiOperation(
            operationId: "oauthAuthPost",
            tags: ["OAuth2"],
            Summary = "OAuth2 Auth Endpoint",
            Description = "OAuth2 authorization endpoint using POST")]
        [OpenApiRequestBody("application/x-www-form-urlencoded", typeof(OauthAuthRequest), Description = "Auth request")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Redirect, Description = "Redirect to the consent page or the redirect URI")]
        public async Task<IActionResult> Oauth_Authorize_Post(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/oauth/authorize")] HttpRequest req,
            FunctionContext context)
        {
            var auth = context.Features.Get<JwtAuthFeature>();
            return await _svc.HandleAuthRequest(req, auth);
        }
    }
}

