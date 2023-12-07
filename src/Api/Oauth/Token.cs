using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.OpenApi.Models;
using Aire.Id.Oauth2;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

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
        [OpenApiOperation(operationId: "Oauth_Token_Post", tags: new[] { "OAuth2" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Oauth_Token_Post(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "oauth/token")] HttpRequest req)
        {
            return await _svc.HandleTokenRequest(req);
        }

        [Function("Oauth_TokenInfo_Get")]
        [OpenApiOperation(operationId: "Oauth_TokenInfo", tags: new[] { "OAuth2" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public IActionResult Oauth_TokenInfo(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "oauth/tokeninfo/{token}")] HttpRequest req,
            string token)
        {
            return _svc.HandleTokenInfoRequest(token);
        }
    }
}

