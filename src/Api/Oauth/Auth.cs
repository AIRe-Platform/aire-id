using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Aire.Id.Oauth2;
using Aire.Id.Oauth2.Models;

namespace Aire.Id.Api
{
    public class Auth
    {
        private readonly OauthAuthenticationService _svc;
        private readonly ILogger<Auth> _log;

        public Auth(OauthAuthenticationService svc, ILogger<Auth> log)
        {
            _svc = svc;
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
        [OpenApiResponseWithoutBody(HttpStatusCode.NotImplemented, Description = "The endpoint is not yet implemented")]
        public async Task<IActionResult> Oauth_Authorize_Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "oauth/authorize")] HttpRequest req)
        {
            // TODO: Implement
            _log.LogError("Not implemented!");
            return await Task.FromResult(
                new StatusCodeResult((int) HttpStatusCode.NotImplemented)
            );
        }

        [Function("Oauth_Auth_Post")]
        [OpenApiOperation(
            operationId: "oauthAuthPost", 
            tags: ["OAuth2"],
            Summary = "OAuth2 Auth Endpoint",
            Description = "OAuth2 authorization endpoint using POST")]
        [OpenApiRequestBody("application/x-www-form-urlencoded", typeof(OauthAuthRequest), Description = "Auth request")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotImplemented, Description = "The endpoint is not yet implemented")]
        public async Task<IActionResult> Oauth_Authorize_Post(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "oauth/authorize")] HttpRequest req)
        {
            // TODO: Implement
            _log.LogError("Not implemented!");
            return await Task.FromResult(
                new StatusCodeResult((int) HttpStatusCode.NotImplemented)
            );
        }

        [Function("Oauth_Auth_Callback")]
        [OpenApiOperation(
            operationId: "oauthAuthCallback", 
            tags: ["OAuth2"],
            Summary = "OAuth2 Auth Code Callback",
            Description = "OAuth2 authorization code callback")]
        [OpenApiParameter("code", Description = "Authorization code", In = ParameterLocation.Query)]
        [OpenApiParameter("state", Description = "Authorization request state value", In = ParameterLocation.Query)]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotImplemented, Description = "The endpoint is not yet implemented")]
        public async Task<IActionResult> Oauth_Authorize_Callback(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "oauth/authorize/callback")] HttpRequest req)
        {
            // TODO: Implement
            _log.LogError("Not implemented!");
            return await Task.FromResult(
                new StatusCodeResult((int) HttpStatusCode.NotImplemented)
            );
        }
    }
}

