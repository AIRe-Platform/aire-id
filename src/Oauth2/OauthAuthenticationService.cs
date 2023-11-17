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

        public async Task<IActionResult> HandleTokenRequest(HttpRequest req)
        {
            OauthTokenRequest tokenRequest = null;
            try
            {
                tokenRequest = OauthTokenRequest.FromRequest(req);
                if(tokenRequest is OauthTokenPasswordGrantRequest)
                {
                    return await PasswordGrant(tokenRequest as OauthTokenPasswordGrantRequest);
                }
                // TODO: Add other supported grant types
            }
            catch(OauthException ex)
            {
                return ex.OauthErrorResult();
            }
            catch(Exception ex)
            {
                _log.LogError(ex, "An exception occurred while handling OAuth token request");
                var oex = new OauthException(OauthError.ServerError)
                {
                    State = tokenRequest?.State,
                };
                return oex.OauthErrorResult();
            }

            var unsupported = new OauthException(OauthError.UnsupportedGrantType);
            return unsupported.OauthErrorResult();
        }

        private async Task<IActionResult> PasswordGrant(OauthTokenPasswordGrantRequest req)
        {
            var subject = await _loginProvider.GetUser(req.Username, req.Password);
            if(subject == null)
                throw new OauthException(OauthError.InvalidGrant);

            var desc = new OauthTokenDescription {
                Subject = subject,
                Lifetime = _config.TokenLifetime
            };

            var token = _tokenProvider.IssueNewToken(desc);

            var response = new OauthTokenResponse
            {
                AccessToken = token,
                TokenType = OauthTokenType.Bearer,
                ExpiresIn = (int) _config.TokenLifetime.TotalSeconds
            };

            // TODO: Check scopes and claims

            return new OkObjectResult(response);
        }
    }
}