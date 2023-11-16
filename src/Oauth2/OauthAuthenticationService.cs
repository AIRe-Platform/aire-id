using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Aire.Helpers;
using Aire.Id.Models;
using Aire.Id.Oauth2.Models;
using Aire.Id.Oauth2.Providers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aire.Id.Oauth2
{
    public class OauthAuthenticationService
    {
        private readonly ITableStorageService _storage;
        private readonly IOauthTokenProvider _tokenProvider;
        private readonly IOauthLoginProvider _loginProvider;
        private readonly OauthConfiguration _config;
        private readonly ILogger<OauthAuthenticationService> _log;

        // TODO: Handle Oauth operations here and call the methods from Auth and Token endpoints

        public OauthAuthenticationService(
            ITableStorageService storage, 
            IOauthTokenProvider tokenProvider,
            IOauthLoginProvider loginProvider,
            IOptions<OauthConfiguration> config, 
            ILogger<OauthAuthenticationService> log)
        {
            _storage = storage;
            _tokenProvider = tokenProvider;
            _loginProvider = loginProvider;
            _log = log;
            _config = config.Value;
        }

        public async Task<IActionResult> HandleAuthRequest(OauthAuthRequest req)
        {
            return await Task.FromResult(new NotFoundResult());
        }

        public async Task<IActionResult> HandleTokenRequest(OauthTokenRequest req)
        {
            switch(req.GrantType)
            {
                case OauthGrantType.Password:
                {
                    var subject = await _loginProvider.GetUser(req.Username, req.Password);

                    var desc = new OauthTokenDescription {
                        Subject = subject,
                        Lifetime = _config.TokenLifetime
                    };

                    var token = _tokenProvider.IssueNewToken(desc);

                    var response = new OauthTokenResponse() {
                        AccessToken = token,
                        ExpiresIn = (int) _config.TokenLifetime.TotalSeconds,
                        Scope = string.Join(" ", subject.Scopes),
                        TokenType = OauthTokenType.Bearer
                    };

                    return new OkObjectResult(response);
                }
                default:
                    // TODO: Return appropriate error response
                    return new BadRequestResult();
            }


        }
    }
}