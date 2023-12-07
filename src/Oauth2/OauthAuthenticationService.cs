using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Aire.Helpers;
using Aire.Id.Oauth2.Models;
using Aire.Id.Oauth2.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aire.Id.Oauth2
{
    public static class OauthExtension
    {
        public static IFunctionsWorkerApplicationBuilder UseOauth<TTokenProvider, TLoginProvider>(this IFunctionsWorkerApplicationBuilder builder)
            where TTokenProvider : class, IOauthTokenProvider
            where TLoginProvider : class, IOauthLoginProvider
        {
            builder.Services
                .AddSingleton<IOauthTokenProvider, TTokenProvider>()
                .AddSingleton<IOauthLoginProvider, TLoginProvider>()
                .AddSingleton<OauthAuthenticationService>();
            return builder;
        }
    }

    public class OauthAuthenticationService
    {
        private readonly ITableStorageService _storage;
        private readonly IOauthTokenProvider _tokenProvider;
        private readonly IOauthLoginProvider _loginProvider;
        private readonly OauthConfiguration _config;
        private readonly ILogger<OauthAuthenticationService> _log;

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

        public IActionResult HandleTokenInfoRequest(string token)
        {
            var info = _tokenProvider.GetTokenInfo(token);
            if(info == null)
                return new StatusCodeResult((int) HttpStatusCode.Unauthorized);

            return new OkObjectResult(info);
        }

        private async Task<IActionResult> PasswordGrant(OauthTokenPasswordGrantRequest req)
        {
            var subject = await _loginProvider.GetUser(req.Username, req.Password);
            if(subject == null)
                throw new OauthException(OauthError.InvalidGrant);

            subject.Scopes ??= new();

            if(_config.DefaultScopes != null)
                subject.Scopes.AddRange(_config.DefaultScopes);

            if(subject.Role != null && _config.DefaultRoleScopes != null && _config.DefaultRoleScopes.ContainsKey(subject.Role))
                subject.Scopes.AddRange(_config.DefaultRoleScopes[subject.Role]);

            var desc = new OauthTokenDescription {
                Subject = subject,
                Lifetime = _config.TokenLifetime
            };

            var token = _tokenProvider.IssueNewToken(desc);

            if(!string.IsNullOrWhiteSpace(req.Scope))
            {
                var requestedScopes = req.Scope.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                subject.Scopes = requestedScopes.Where(x => subject.Scopes.Contains(x)).ToList();
            }

            var response = new OauthTokenResponse
            {
                AccessToken = token,
                TokenType = OauthTokenType.Bearer,
                ExpiresIn = (int) _config.TokenLifetime.TotalSeconds,
                Scope = string.Join(" ", subject.Scopes)
            };

            return new OkObjectResult(response);
        }
    }
}