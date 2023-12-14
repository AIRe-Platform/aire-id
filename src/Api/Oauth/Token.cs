using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.OpenApi.Models;
using Aire.Id.Oauth2;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Net;
using Aire.Id.Oauth2.Models;

namespace Aire.Id.Api
{
    public class Token
    {
        private readonly OauthAuthenticationService _svc;

        public Token(OauthAuthenticationService svc)
        {
            _svc = svc;
        }

        [Function("Oauth_Token_Post")]
        [OpenApiOperation(operationId: "Oauth_Token_Post", tags: ["OAuth2"], Description = "OAuth2 Token Endpoint")]
        [OpenApiRequestBody("application/x-www-form-urlencoded", typeof(OauthTokenRequest), Description = "Request depends on the grant type used", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(OauthTokenResponse), Description = "The OK response")]
        [OpenApiResponseWithBody(HttpStatusCode.Forbidden, "application/json", typeof(OauthErrorResponse), Description = "OAuth error response", Summary = "Access denied")]
        [OpenApiResponseWithBody(HttpStatusCode.Unauthorized, "application/json", typeof(OauthErrorResponse), Description = "OAuth error response", Summary = "Unauthorized client")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(OauthErrorResponse), Description = "OAuth error response")]
        [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(OauthErrorResponse), Description = "OAuth error response")]
        [OpenApiResponseWithBody(HttpStatusCode.ServiceUnavailable, "application/json", typeof(OauthErrorResponse), Description = "OAuth error response")]
        public async Task<IActionResult> Oauth_Token_Post(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "oauth/token")] HttpRequest req)
        {
            return await _svc.HandleTokenRequest(req);
        }

        [Function("Oauth_TokenInfo_Get")]
        [OpenApiOperation(operationId: "Oauth_TokenInfo", tags: ["OAuth2"], Description = "Verify token and get its information")]
        [OpenApiParameter("token", Description = "The JWT token to verify", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(OauthTokenResponse), Description = "Token response")]
        public IActionResult Oauth_TokenInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "oauth/tokeninfo/{token}")] HttpRequest req,
            string token)
        {
            return _svc.HandleTokenInfoRequest(token);
        }
    }
}

