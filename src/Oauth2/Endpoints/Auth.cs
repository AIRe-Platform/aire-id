using System.IO;
using System.Net;
using System.Threading.Tasks;
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
    public class Auth
    {
        private readonly OauthAuthenticationService _svc;
        private readonly ILogger<Auth> _log;

        public Auth(OauthAuthenticationService svc, ILogger<Auth> log)
        {
            _svc = svc;
            _log = log;
        }

        [FunctionName("Oauth_Auth")]
        [OpenApiOperation(operationId: "Oauth_Auth", tags: new[] { "OAuth2" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Oauth_Authorize(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "oauth/auth")] HttpRequest req)
        {
            // TODO: Implement
            return await Task.FromResult(new NotFoundResult());
        }
    }
}

