// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


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
using Aire.Sdk.Platform;

namespace Aire.Id.Oauth2;

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

public class OauthAuthenticationService(
    ITableStorageService storage,
    IOauthTokenProvider tokenProvider,
    IOauthLoginProvider loginProvider,
    IAirePlatformService platformService,
    IOptions<OauthConfiguration> config,
    IJwtTokenService jwt,
    ILogger<OauthAuthenticationService> log)
{
    private readonly ITableStorageService _storage = storage;
    private readonly IOauthTokenProvider _tokenProvider = tokenProvider;
    private readonly IOauthLoginProvider _loginProvider = loginProvider;
    private readonly IAirePlatformService _platformService = platformService;
    private readonly OauthConfiguration _config = config.Value;
    private readonly IJwtTokenService _jwt = jwt;
    private readonly ILogger<OauthAuthenticationService> _log = log;

    public async Task<IActionResult> HandleAuthRequest(HttpRequest req, JwtAuthFeature? auth)
    {
        OauthAuthRequest? authRequest = null;
        try
        {
            authRequest = OauthAuthRequest.FromRequest(req);
            if (authRequest != null)
            {
                if (auth != null)
                {
                    if (!_jwt.CheckAuthorization(auth, AireScopes.Auth))
                        throw new OauthException(OauthError.AccessDenied, authRequest);

                    return authRequest.ResponseType switch
                    {
                        OauthResponseType.Code => await AuthorizationCodeResponse(authRequest, auth),
                        _ => throw new OauthException(OauthError.UnsupportedResponseType, authRequest),
                    };
                }
                else
                {
                    return await RedirectToLoginPage(authRequest, req);
                }
            }
            else
            {
                throw new OauthException(OauthError.InvalidRequest, "Invalid or malformed request.");
            }
        }
        catch (OauthException ex)
        {
            return ex.OauthErrorResult();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An exception occurred while handling OAuth token request");
            var oex = new OauthException(OauthError.ServerError, authRequest, ex.Message);
            return oex.OauthErrorResult();
        }
    }

    public async Task<IActionResult> HandleTokenRequest(HttpRequest req)
    {
        OauthTokenRequest? tokenRequest = null;
        try
        {
            tokenRequest = OauthTokenRequest.FromRequest(req);
            if (tokenRequest != null)
            {
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
            else
            {
                throw new OauthException(OauthError.InvalidRequest, "Invalid or malformed request.");
            }
        }
        catch (OauthException ex)
        {
            return ex.OauthErrorResult();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "An exception occurred while handling OAuth token request");
            var oex = new OauthException(OauthError.ServerError, tokenRequest, ex.Message);
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
        var client = await _storage.RetrieveAsync<ClientEntity>(req.ClientId!)
            ?? throw new OauthException(OauthError.UnauthorizedClient, req, "Invalid client ID");

        if (!client.Active)
            throw new OauthException(OauthError.UnauthorizedClient, req, "Client deactivated");

        if (!VerifyClientSecret(client.SecretHash, req.ClientSecret))
            throw new OauthException(OauthError.UnauthorizedClient, req, "Missing or invalid client secret");

        var scopes = ValidateClientScopes(req.Scope, client)
            ?? throw new OauthException(OauthError.InvalidScope, req, "Invalid scopes requested");

        if (!ValidateGrantType(client, req.GrantType))
            throw new OauthException(OauthError.InvalidGrant, req, "Grant type not allowed");

        var subject = await _loginProvider.Login(req.Username!, req.Password!)
            ?? throw new OauthException(OauthError.AccessDenied, req, "Invalid credentials");

        if (req.Platform != null)
            subject.Claims.Add(AireClaims.Platform, req.Platform);

        if (subject.AllowedScopes != null)
            scopes = scopes.Where(x => subject.AllowedScopes.Contains(x)).ToArray();

        if (!string.IsNullOrWhiteSpace(req.Scope))
        {
            var requestedScopes = AireScopes.ParseString(req.Scope);
            scopes = requestedScopes.Where(x => scopes.Contains(x)).ToArray();
        }

        var desc = new OauthTokenDescription(subject, [.. scopes], _config.TokenLifetime);
        var token = _tokenProvider.IssueNewToken(desc);

        var response = new OauthTokenResponse
        {
            AccessToken = token,
            TokenType = OauthTokenType.Bearer,
            ExpiresIn = (int)_config.TokenLifetime.TotalSeconds,
            Scope = string.Join(" ", scopes),
            State = req.State,
        };

        return new OkObjectResult(response);
    }

    private async Task<IActionResult> AuthCodeGrant(OauthAuthCodeGrantRequest req)
    {
        var client = await _storage.RetrieveAsync<ClientEntity>(req.ClientId!)
            ?? throw new OauthException(OauthError.UnauthorizedClient, req, "Invalid client ID");

        if (!client.Active)
            throw new OauthException(OauthError.UnauthorizedClient, req, "Client deactivated");

        if (!VerifyClientSecret(client.SecretHash, req.ClientSecret))
            throw new OauthException(OauthError.UnauthorizedClient, req, "Missing or invalid client secret");

        if (!ValidateGrantType(client, req.GrantType))
            throw new OauthException(OauthError.InvalidGrant, req, "Grant type not allowed");

        var code = await _storage.RetrieveAsync<AuthCodeEntity>(req.Code![..5], req.Code!)
            ?? throw new OauthException(OauthError.InvalidGrant, req, "Invalid code");

        if (!await _storage.DeleteAsync(code))
            _log.LogError("Failed to delete auth code entity");

        if (req.State != code.State || req.RedirectUri != code.RedirectUri || req.ClientId != code.ClientId)
            throw new OauthException(OauthError.InvalidGrant, req, "Invalid grant"); ;

        if (code.Verifier != null)
        {
            if (string.IsNullOrEmpty(req.CodeVerifier))
                throw new OauthException(OauthError.InvalidGrant, req, "Missing code verifier");

            if (!Crypto.SHA256Base16(req.CodeVerifier).Equals(code.Verifier, StringComparison.CurrentCultureIgnoreCase))
                throw new OauthException(OauthError.InvalidGrant, req, "Invalid code verifier");
        }

        if (req.Platform != code.Platform)
            throw new OauthException(OauthError.InvalidGrant, req, "Invalid platform");

        if (code.Expires < DateTime.UtcNow)
            throw new OauthException(OauthError.InvalidGrant, req, "Expired code");

        var user = await _storage.RetrieveAsync<UserEntity>(code.UserId!)
            ?? throw new OauthException(OauthError.InvalidGrant, req, "Expired code");

        var subject = _loginProvider.GetSubject(user, code.UserKey!, code.Platform);
        if (code.Platform != null)
            subject.Claims.Add(AireClaims.Platform, code.Platform);

        var scopes = AireScopes.ParseString(code.Scopes ?? "");

        var tokenDescription = new OauthTokenDescription(subject, scopes, _config.TokenLifetime);
        var token = _tokenProvider.IssueNewToken(tokenDescription);

        var response = new OauthTokenResponse
        {
            AccessToken = token,
            TokenType = OauthTokenType.Bearer,
            ExpiresIn = (int)_config.TokenLifetime.TotalSeconds,
            Scope = code.Scopes,
            State = code.State,
        };

        return new OkObjectResult(response);
    }

    private async Task<IActionResult> RedirectToLoginPage(OauthAuthRequest req, HttpRequest httpRequest)
    {
        var client = await _storage.RetrieveAsync<ClientEntity>(req.ClientId!)
            ?? throw new OauthException(OauthError.UnauthorizedClient, req, "Invalid client ID");

        if (req.Platform == null)
            throw new OauthException(OauthError.InvalidRequest, "Platform required");

        ThrowIfInvalidClientPlatform(client, req.Platform);

        var platform = await _platformService.GetInternalPlatformConfiguration(req.Platform)
            ?? throw new OauthException(OauthError.TemporarilyUnavailable, "Platform unavailable");

        var scopes = ValidateClientScopes(req.Scope, client)
            ?? throw new OauthException(OauthError.InvalidScope, req, "Invalid scopes requested");

        var query = httpRequest.Query.ToDictionary();
        query["service"] = client.Name;
        query["response_type"] = req.ResponseType.ObjectToJson().Trim('"');
        query["client_id"] = req.ClientId;
        query["redirect_uri"] = GetClientRedirectUri(req, client).AbsoluteUri;
        query["scope"] = string.Join(" ", scopes);
        query["state"] = req.State;
        query["consent"] = client.RequireConsent ? "1" : "0";
        query["platform"] = req.Platform;
        query["platform_name"] = platform.Platform?.Name;

        if (req.CodeChallenge != null)
            query["code_challenge"] = req.CodeChallenge;

        if (req.CodeChallengeMethod != null)
            query["code_challenge_method"] = req.CodeChallengeMethod.ObjectToJson().Trim('"');

        var uri = QueryHelpers.AddQueryString(AireConstants.AppAuthPath, query);
        return new RedirectResult(uri, false, false);
    }

    private async Task<IActionResult> AuthorizationCodeResponse(OauthAuthRequest req, JwtAuthFeature auth)
    {
        var client = await _storage.RetrieveAsync<ClientEntity>(req.ClientId!)
            ?? throw new OauthException(OauthError.UnauthorizedClient, req, "Invalid client ID");

        if (!client.Active)
            throw new OauthException(OauthError.UnauthorizedClient, req, "Client deactivated");

        if (req.Platform == null)
            throw new OauthException(OauthError.InvalidRequest, "Platform required");

        ThrowIfInvalidClientPlatform(client, req.Platform);

        var user = await _storage.RetrieveAsync<UserEntity>(auth.UserId)
            ?? throw new OauthException(OauthError.AccessDenied, req, "Access denied");

        var scopes = ValidateClientScopes(req.Scope, client)
            ?? throw new OauthException(OauthError.InvalidScope, req, "Invalid scopes requested");

        scopes = FilterUserScopes(user, scopes, req.Platform);

        var redirect = GetClientRedirectUri(req, client);
        string code = RandomNumberGenerator.GetHexString(32, true);

        if (!req.CodeChallengeMethod.HasValue)
            throw new OauthException(OauthError.InvalidRequest, req, "Code challenge method required");

        if (string.IsNullOrWhiteSpace(req.CodeChallenge))
            throw new OauthException(OauthError.InvalidRequest, req, "Missing code challenge");

        string? codeVerifier = req.CodeChallengeMethod switch
        {
            OauthCodeChallengeMethod.SHA256 => req.CodeChallenge.ToLower(),
            _ => throw new OauthException(OauthError.InvalidRequest, req, "Unsupported code challenge method"),
        };

        var codeEntity = new AuthCodeEntity(code)
        {
            ClientId = client.Id(),
            ClientSecretHash = client.Public ? null : client.SecretHash,
            UserId = auth.UserId,
            UserKey = auth.UserKey,
            Scopes = string.Join(" ", scopes),
            State = req.State,
            RedirectUri = GetClientRedirectUri(req, client).AbsoluteUri,
            Verifier = codeVerifier,
            Expires = DateTime.UtcNow.AddMinutes(5),
            Platform = req.Platform
        };

        bool created = await _storage.UpsertAsync(codeEntity);
        if (!created)
            throw new OauthException(OauthError.TemporarilyUnavailable, req, "Internal error");

        var query = HttpUtility.ParseQueryString(redirect.Query);
        query["code"] = code;
        query["state"] = req.State;

        var redirectUri = new UriBuilder(redirect)
        {
            Query = query.ToString()
        }.Uri.AbsoluteUri;

        return new RedirectResult(redirectUri, false, false);
    }

    private static Uri GetClientRedirectUri(OauthAuthRequest req, ClientEntity client)
    {
        if (string.IsNullOrWhiteSpace(client.RedirectUri))
            throw new OauthException(OauthError.TemporarilyUnavailable, req, "Client is not configured correctly.");

        if (string.IsNullOrWhiteSpace(req.RedirectUri))
            return new Uri(client.RedirectUri);

        var reqUri = new Uri(req.RedirectUri);
        var clientUri = new Uri(client.RedirectUri);

        if (reqUri.Scheme != clientUri.Scheme ||
            reqUri.Host != clientUri.Host ||
            reqUri.AbsolutePath != clientUri.AbsolutePath)
        {
            req.RedirectUri = null;
            throw new OauthException(OauthError.InvalidRequest, req, "Redirect URI does not match.");
        }

        return reqUri;
    }

    public static string[]? ValidateClientScopes(string? reqScopes, ClientEntity client)
    {
        var scopes = reqScopes != null ? AireScopes.ParseString(reqScopes) : null;

        var clientScopes = client.AllowedScopes != null
            ? new AireScopes(client.GetAllowedScopes())
            : null;

        bool allowAllClientScopes = clientScopes?.Contains("*") ?? false;
        bool grantAllAvailableScopes = scopes == null || scopes.Contains("*") || scopes.Count == 0;

        if (grantAllAvailableScopes)
            scopes = clientScopes;

        scopes ??= [];

        // Check that client scopes include requested scopes
        if (clientScopes != null)
        {
            var not_allowed = scopes.Where(x => !clientScopes.Contains(x)).ToList();
            if (not_allowed.Count > 0)
                return null;
        }

        return [.. scopes];
    }

    public static string[] FilterUserScopes(UserEntity user, IEnumerable<string> requested, string platform)
    {
        var scopes = user.GetScopes(platform);

        // Ignore all scopes not allowed for the user
        return [.. requested.Where(x => scopes.Contains(x))];
    }

    public static bool ValidateGrantType(ClientEntity client, OauthGrantType grantType)
    {
        var allowed = client.GetGrantTypes();
        var requested = grantType.ObjectToJson().Trim('"');
        return allowed.Contains(requested);
    }

    public static bool VerifyClientSecret(string? secret_hash, string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret_hash))
            return true;

        if (string.IsNullOrWhiteSpace(secret))
            return false;

        var hash = Crypto.SHA256Base64(secret);
        return hash == secret_hash;
    }

    private static void ThrowIfInvalidClientPlatform(ClientEntity client, string platform)
    {
        var allowedPlatforms = client.GetAllowedPlatforms();
        if (!allowedPlatforms.Contains(platform) && allowedPlatforms.FirstOrDefault() != "*")
            throw new OauthException(OauthError.UnauthorizedClient, "Client not authorized for platform");
    }
}
