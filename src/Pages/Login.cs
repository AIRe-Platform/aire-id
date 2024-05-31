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
public class LoginPage
{
    private readonly IJwtTokenService _jwt;
    private readonly IOauthLoginProvider _loginProvider;
    private readonly ILogger<LoginPage> _log;

    private const string AuthScope = "authorize";

    public LoginPage(IJwtTokenService jwt, IOauthLoginProvider loginProvider, ILogger<LoginPage> log)
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

    [Function("GetLoginPage")]
    [OpenApiIgnore]
    public IActionResult GetLoginPage([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "login")] HttpRequest req)
    {
        var path = Path.Combine(Environment.CurrentDirectory, $"www/pages/login.html");

        if(!File.Exists(path))
            return new NotFoundResult();

        var stream = File.OpenRead(path);
        if(!stream.CanRead)
        {
            _log.LogError("Cannot read file {filePath}", path);
            return new InternalServerErrorResult();
        }

        return new FileStreamResult(stream, "text/html");
    }

    [Function("ValidateSession")]
    [OpenApiIgnore]
    public async Task<IActionResult> ValidateSession([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login/session")] HttpRequest req)
    {
        var session = await req.ReadJson<Session>();
        if(session == null || string.IsNullOrWhiteSpace(session.Token))
            return new BadRequestResult();

        var token = _jwt.ValidateToken(session.Token);
        if(token == null)
            return new UnauthorizedResult();

        return new OkResult();
    }

    [Function("PostLoginForm")]
    [OpenApiIgnore]
    public IActionResult PostLoginForm([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login")] HttpRequest req)
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
