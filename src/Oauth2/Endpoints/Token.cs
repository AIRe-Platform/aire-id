using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Aire.Helpers;
using Aire.Id.Models;
using Aire.Id.Oauth2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace Aire.Id.Oauth2
{
    public class Token
    {
        private readonly OauthAuthenticationService _svc;
        private readonly IJwtTokenService _jwt;
        private readonly ILogger<Token> _log;

        public Token(OauthAuthenticationService svc, IJwtTokenService jwt, ILogger<Token> log)
        {
            _svc = svc;
            _jwt = jwt;
            _log = log;
        }

        [FunctionName("Oauth_Token_Get")]
        [OpenApiOperation(operationId: "Oauth_Token", tags: new[] { "OAuth2" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Oauth_Token_Get(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "oauth/token")] HttpRequest req)
        {
            OauthTokenRequest request = null;

            // TODO: Parse query

            return await _svc.HandleTokenRequest(request);
        }

        [FunctionName("Oauth_Token_Post")]
        [OpenApiOperation(operationId: "Oauth_Token_Post", tags: new[] { "OAuth2" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Oauth_Token_Post(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "oauth/token")] HttpRequest req)
        {
            var request = new OauthTokenRequest(req);
            return await _svc.HandleTokenRequest(request);
        }

        [FunctionName("Oauth_TokenInfo_Get")]
        [OpenApiOperation(operationId: "Oauth_TokenInfo", tags: new[] { "OAuth2" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Oauth_TokenInfo(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "oauth/tokeninfo")] HttpRequest req)
        {
            return await Task.FromResult(new NotFoundResult());
        }
    }
}

