using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using Aire.Id.Oauth2;
using Aire.Id.Oauth2.Models;
using Aire.Sdk.AspNetCore;

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
        [OpenApiOperation(
            operationId: "oauthTokenPost", 
            tags: ["OAuth2"], 
            Summary = "OAuth2 Token Endpoint")]
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

        public class TokenInfoRequestBody
        {
            [OpenApiProperty(Description = "Access token", Nullable = false)]
            public string? Token { get; set; }
        }

        [Function("Oauth_TokenInfo")]
        [OpenApiOperation(
            operationId: "oauthTokenInfo", 
            tags: ["OAuth2"],
            Summary = "Verify token",
            Description = "Decodes and verifies given token")]
        [OpenApiRequestBody("application/x-www-form-urlencoded", typeof(TokenInfoRequestBody), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(OauthTokenResponse), Description = "Token response")]
        public IActionResult Oauth_TokenInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "oauth/tokeninfo")] HttpRequest req)
        {
            var token = req.ReadParam("token");

            if(string.IsNullOrEmpty(token))
                return new BadRequestResult();

            return _svc.HandleTokenInfoRequest(token);
        }
    }
}

