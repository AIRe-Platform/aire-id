using System.Net;
using Aire.Sdk.Azure;
using Aire.Id.Oauth2.Models;
using Aire.Id.Oauth2.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Aire.Sdk.Auth;
using System.Security.Cryptography;
using Aire.Id.Models;
using Aire.Sdk.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Web;

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
        private readonly IJwtTokenService _jwt;
        private readonly ILogger<OauthAuthenticationService> _log;

        public OauthAuthenticationService(
            ITableStorageService storage,
            IOauthTokenProvider tokenProvider,
            IOauthLoginProvider loginProvider,
            IOptions<OauthConfiguration> config,
            IJwtTokenService jwt,
            ILogger<OauthAuthenticationService> log)
        {
            _storage = storage;
            _tokenProvider = tokenProvider;
            _loginProvider = loginProvider;
            _config = config.Value;
            _jwt = jwt;
            _log = log;
        }

        public async Task<IActionResult> HandleAuthRequest(HttpRequest req, JwtAuthFeature? auth)
        {
            OauthAuthRequest? authRequest = null;
            try
            {
                authRequest = OauthAuthRequest.FromRequest(req);
                if (authRequest != null)
                {
                    if (auth != null)
                        switch (authRequest.ResponseType)
                        {
                            case OauthResponseType.Code:
                                return await AuthorizationCodeResponse(authRequest, auth);
                            default:
                                throw new OauthException(OauthError.UnsupportedResponseType);
                        }
                    else
                        return await RedirectToLoginPage(authRequest, req);
                }
            }
            catch (OauthException ex)
            {
                return ex.OauthErrorResult();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An exception occurred while handling OAuth token request");
                var oex = new OauthException(OauthError.ServerError)
                {
                    Redirect = authRequest?.RedirectUri,
                    State = authRequest?.State,
                };
                return oex.OauthErrorResult();
            }

            var unsupported = new OauthException(OauthError.UnsupportedResponseType);
            return unsupported.OauthErrorResult();
        }

        public async Task<IActionResult> HandleTokenRequest(HttpRequest req)
        {
            OauthTokenRequest? tokenRequest = null;
            try
            {
                tokenRequest = OauthTokenRequest.FromRequest(req);
                if (tokenRequest != null)
                {
                    // TODO: Disable password grant
                    if (tokenRequest is OauthTokenPasswordGrantRequest)
                    {
                        return await PasswordGrant((OauthTokenPasswordGrantRequest)tokenRequest!);
                    }
                    else if (tokenRequest is OauthAuthCodeGrantRequest)
                    {
                        return await AuthCodeGrant((OauthAuthCodeGrantRequest)tokenRequest!);
                    }
                    // else if (tokenRequest is OauthTokenRefreshRequest)
                    // {
                    //     return await RefreshTokenGrant((OauthTokenRefreshRequest) tokenRequest!);
                    // }
                }
            }
            catch (OauthException ex)
            {
                return ex.OauthErrorResult();
            }
            catch (Exception ex)
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
            if (info == null)
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);

            return new OkObjectResult(info);
        }

        private async Task<IActionResult> PasswordGrant(OauthTokenPasswordGrantRequest req)
        {
            var subject = await _loginProvider.Login(req.Username!, req.Password!);
            if (subject == null)
                throw new OauthException(OauthError.InvalidGrant);

            subject.Scopes ??= [];
            if (!string.IsNullOrWhiteSpace(req.Scope))
            {
                var requestedScopes = req.Scope.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                subject.Scopes = requestedScopes.Where(x => subject.Scopes.Contains(x)).ToList();
            }

            var desc = new OauthTokenDescription
            {
                Subject = subject,
                Lifetime = _config.TokenLifetime
            };

            var token = _tokenProvider.IssueNewToken(desc);

            var response = new OauthTokenResponse
            {
                AccessToken = token,
                TokenType = OauthTokenType.Bearer,
                ExpiresIn = (int)_config.TokenLifetime.TotalSeconds,
                Scope = string.Join(" ", subject.Scopes),
                State = req.State
            };

            return new OkObjectResult(response);
        }

        private async Task<IActionResult> AuthCodeGrant(OauthAuthCodeGrantRequest req)
        {
            var code = await _storage.RetrieveAsync<AuthCodeEntity>(req.Code![..5], req.Code!);
            if (code == null)
                throw new OauthException(OauthError.InvalidGrant)
                {
                    Redirect = req.RedirectUri,
                    State = req.State
                };

            if (code.Expires < DateTime.UtcNow)
                throw new OauthException(OauthError.InvalidGrant)
                {
                    Redirect = req.RedirectUri,
                    State = req.State
                };

            if (req.State != code.State || req.RedirectUri != code.RedirectUri || req.ClientId != code.ClientId)
                throw new OauthException(OauthError.InvalidGrant)
                {
                    Redirect = req.RedirectUri,
                    State = req.State
                };

            var user = await _storage.RetrieveAsync<UserEntity>(code.UserId!);
            if (user == null)
                throw new OauthException(OauthError.InvalidGrant)
                {
                    Redirect = req.RedirectUri,
                    State = req.State
                };

            var tokenDescription = new OauthTokenDescription
            {
                Lifetime = _config.TokenLifetime,
                Subject = _loginProvider.GetSubject(user, code.UserKey!)
            };

            var token = _tokenProvider.IssueNewToken(tokenDescription);

            var response = new OauthTokenResponse
            {
                AccessToken = token,
                TokenType = OauthTokenType.Bearer,
                ExpiresIn = (int)_config.TokenLifetime.TotalSeconds,
                Scope = code.Scopes,
                State = code.State
            };

            return new OkObjectResult(response);
        }

        private async Task<IActionResult> RedirectToLoginPage(OauthAuthRequest req, HttpRequest httpRequest)
        {
            var client = await _storage.RetrieveAsync<ClientEntity>(req.ClientId!);
            if (client == null)
            {
                throw new OauthException(OauthError.UnauthorizedClient)
                {
                    Redirect = req.RedirectUri,
                    State = req.State
                };
            }

            if (!Uri.TryCreate(AireEnvironment.AuthLoginRedirect, UriKind.Absolute, out Uri? redirect_uri))
            {
                _log.LogError("AUTH_LOGIN_REDIRECT is not configured properly");
                throw new OauthException(OauthError.TemporarilyUnavailable)
                {
                    Redirect = req.RedirectUri,
                    State = req.State
                };
            }

            var query = httpRequest.Query.ToDictionary();
            query["service"] = client.Name;
            query["response_type"] = req.ResponseType.ObjectToJson();
            query["client_id"] = req.ClientId;
            query["redirect_uri"] = GetClientRedirectUri(req, client).AbsoluteUri;
            query["scope"] = string.Join(" ", ValidateScopes(req, client, null));
            query["state"] = req.State;
            var uri = QueryHelpers.AddQueryString(redirect_uri.AbsoluteUri, query);
            return new RedirectResult(uri, false, false);
        }

        private async Task<IActionResult> AuthorizationCodeResponse(OauthAuthRequest req, JwtAuthFeature auth)
        {
            var client = await _storage.RetrieveAsync<ClientEntity>(req.ClientId!);
            if (client == null)
                throw new OauthException(OauthError.UnauthorizedClient)
                {
                    Redirect = req.RedirectUri,
                    State = req.State
                };

            var scopes = ValidateScopes(req, client, auth);
            var redirect = GetClientRedirectUri(req, client);
            string code = RandomNumberGenerator.GetHexString(32, true);

            var codeEntity = new AuthCodeEntity(code)
            {
                ClientId = client.Id(),
                UserId = auth.UserId,
                UserKey = auth.UserKey,
                Scopes = string.Join(" ", scopes),
                State = req.State,
                RedirectUri = req.RedirectUri,
                Expires = DateTime.UtcNow.AddMinutes(5)
            };

            bool created = await _storage.UpsertAsync(codeEntity);
            if (!created)
                throw new OauthException(OauthError.TemporarilyUnavailable)
                {
                    Redirect = req.RedirectUri,
                    State = req.State
                };

            var query = HttpUtility.ParseQueryString(redirect.Query);
            query["code"] = code;
            query["state"] = req.State;

            var redirectUri = new UriBuilder(redirect)
            {
                Query = query.ToString()
            }.Uri.AbsoluteUri;

            return new RedirectResult(redirectUri, false, false);
        }

        private Uri GetClientRedirectUri(OauthAuthRequest req, ClientEntity client)
        {
            if (string.IsNullOrWhiteSpace(client.RedirectUri))
            {
                _log.LogError($"Client '{client.Id()}' is missing property RedirectUri");
                throw new OauthException(OauthError.TemporarilyUnavailable)
                {
                    Redirect = req.RedirectUri,
                    State = req.State
                };
            }

            if (string.IsNullOrWhiteSpace(req.RedirectUri))
                return new Uri(client.RedirectUri);

            var reqUri = new Uri(req.RedirectUri);
            var clientUri = new Uri(client.RedirectUri);

            if (reqUri.Host != clientUri.Host || reqUri.AbsolutePath != clientUri.AbsolutePath)
            {
                _log.LogError($"Requested redirect URI host and path must match with the configured client's URI");
                throw new OauthException(OauthError.InvalidRequest)
                {
                    Redirect = req.RedirectUri,
                    State = req.State
                };
            }

            return reqUri;
        }

        private string[] ValidateScopes(OauthAuthRequest req, ClientEntity client, JwtAuthFeature? auth)
        {
            bool automaticScopes = false;
            var scopes = req.Scope?.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var clientScopes = client.AllowedScopes?.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (clientScopes != null && clientScopes.FirstOrDefault() == "*")
                clientScopes = null;

            if (scopes != null && scopes.FirstOrDefault() == "*")
            {
                automaticScopes = true;
                scopes = clientScopes;
            }

            scopes ??= [];

            // Check client scopes
            if (clientScopes != null)
            {
                var not_allowed = scopes.Where(x => !clientScopes.Contains(x)).ToList();

                if (not_allowed.Count > 0)
                    throw new OauthException(OauthError.AccessDenied)
                    {
                        Redirect = req.RedirectUri,
                        State = req.State
                    };
            }

            // Check user scopes
            if (auth != null)
            {
                if (automaticScopes)
                {
                    scopes = scopes.Where(x => _jwt.CheckAuthorization(auth, x)).ToArray();
                }
                else
                {
                    if (!_jwt.CheckAuthorization(auth, new AireScopes(scopes)))
                        throw new OauthException(OauthError.AccessDenied)
                        {
                            Redirect = req.RedirectUri,
                            State = req.State
                        };
                }
            }

            return scopes;
        }
    }
}