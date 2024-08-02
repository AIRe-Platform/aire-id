using System.Web.Http;
using Aire.Id.Oauth2.Providers;
using Aire.Sdk.AspNetCore;
using Aire.Sdk.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Aire.Id.Pages;

/// <summary>
/// Login page for OAuth 2.0 authentication code flow
/// </summary>
public class Login_v1
{
    private readonly IJwtTokenService _jwt;
    private readonly IOauthLoginProvider _loginProvider;
    private readonly ILogger<Login_v1> _log;

    private const string AuthScope = "authorize";

    public Login_v1(IJwtTokenService jwt, IOauthLoginProvider loginProvider, ILogger<Login_v1> log)
    {
        _jwt = jwt;
        _loginProvider = loginProvider;
        _log = log;
    }

    class LoginForm
    {
        [JsonProperty("username", Required = Required.Always)]
        public string? Username { get; set; }

        [JsonProperty("password", Required = Required.Always)]
        public string? Password { get; set; }
    }

    class Session
    {
        [JsonProperty("token", Required = Required.Always)]
        public string? Token { get; set; }
    }

    [Function("ValidateSession")]
    [OpenApiIgnore]
    public async Task<IActionResult> ValidateSession([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login/validate")] HttpRequest req)
    {
        var session = await req.ReadJson<Session>();
        if(session == null || string.IsNullOrWhiteSpace(session.Token))
            return new BadRequestResult();

        var token = _jwt.ValidateToken(session.Token);
        if(token == null)
            return new UnauthorizedResult();

        return new OkResult();
    }

    [Function("PostLogin")]
    [OpenApiIgnore]
    public IActionResult PostLogin([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login")] HttpRequest req)
    {
        return new NotFoundResult();
    }

    [Function("PostConsent")]
    [OpenApiIgnore]
    public IActionResult PostConsent([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login/consent")] HttpRequest req)
    {
        return new NotFoundResult();
    }
}
