// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Id.Oauth2.Models;
using Aire.Id.Oauth2.Providers;
using Aire.Sdk.AspNetCore;
using Aire.Sdk.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;

namespace Aire.Id.Api;

public class Login_v1
{
    private readonly IOauthLoginProvider _loginProvider;
    private readonly IOauthTokenProvider _tokenProvider;

    public Login_v1(IOauthLoginProvider loginProvider, IOauthTokenProvider tokenProvider)
    {
        _loginProvider = loginProvider;
        _tokenProvider = tokenProvider;
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

        [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Verified { get; set; }
    }

    /// <summary>
    /// Check login session authentication
    /// </summary>
    [Function("GetLoginAuth_v1")]
    [OpenApiIgnore]
    public static IActionResult GetLoginAuth(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/login/auth")] HttpRequest req,
        FunctionContext context)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();
        
        var session = new Session {
            Token = auth.JwtEncodedToken
        };
        
        return new OkObjectResult(session);
    }

    /// <summary>
    /// Login using user credentials
    /// </summary>
    /// <returns>Authentication session token</returns>
    [Function("PostLogin_v1")]
    [OpenApiIgnore]
    public async Task<IActionResult> PostLogin([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/login")] HttpRequest req)
    {
        var form = await req.ReadJson<LoginForm>();
        if (form == null || string.IsNullOrWhiteSpace(form.Username) || string.IsNullOrWhiteSpace(form.Password))
            return new BadRequestResult();

        var subject = await _loginProvider.Login(form.Username, form.Password);
        if (subject == null)
            return new ForbiddenResult();

        if (subject.Verified)
            subject.Scopes = [AireScopes.Auth];
        else
            subject.Scopes = [];

        var desc = new OauthTokenDescription
        {
            Subject = subject,
            Lifetime = AireConstants.AppAuthSessionTTL
        };

        var token = _tokenProvider.IssueNewToken(desc);

        var session = new Session
        {
            Token = token,
            Verified = subject.Verified
        };

        return new OkObjectResult(session);
    }
}
