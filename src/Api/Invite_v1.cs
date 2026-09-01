// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using System.Net;
using Aire.Id.Helpers;
using Aire.Id.Models;
using Aire.Id.Oauth2;
using Aire.Id.Oauth2.Models;
using Aire.Id.Oauth2.Providers;
using Aire.Sdk.AspNetCore;
using Aire.Sdk.Audit;
using Aire.Sdk.Auth;
using Aire.Sdk.Azure;
using Aire.Sdk.Helpers;
using Aire.Sdk.Models.Invites;
using Aire.Sdk.Models.Platform;
using Aire.Sdk.Platform;
using Aire.Sdk.Platform.Clients;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Aire.Id.Api;

public class Invite_v1(
    IJwtTokenService jwt, ITableStorageService storage, IAireClientFactory clientFactory,
    IOauthLoginProvider loginProvider, IOauthTokenProvider tokenProvider,
    QueueServiceClient queues, IAirePlatformService platformService,
    IAireAuditService auditService, ILogger<Invite_v1> log)
{
    private readonly IJwtTokenService _jwt = jwt;
    private readonly ITableStorageService _storage = storage;
    private readonly IAireClientFactory _clientFactory = clientFactory;
    private readonly IAirePlatformService _platformService = platformService;
    private readonly IOauthLoginProvider _loginProvider = loginProvider;
    private readonly IOauthTokenProvider _tokenProvider = tokenProvider;
    private readonly QueueClient _mailQueue = queues.GetQueueClient(AireConstants.Queues.Mail);
    private readonly IAireAuditService _auditService = auditService;
    private readonly ILogger<Invite_v1> _log = log;
    private const string resType = AireAuditResource.InviteCodeResourceType;

    [Function("GetInviteCodes_v1")]
    [OpenApiOperation(
        operationId: "getInviteCodeInfo",
        tags: ["Invites"],
        Summary = "Create invite codes")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiParameter("active_only", In = ParameterLocation.Query, Description = "Return only active codes", Required = false)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<InviteCode>), Description = "Information about invite code")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Authorization required")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    public async Task<IActionResult> GetInviteCodes(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/invite-codes")] HttpRequest req,
        FunctionContext context,
        [FromQuery] bool? active_only,
        [FromQuery] string? platform)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.AdminInvites))
            return new ForbiddenResult();

        active_only ??= false;
        platform ??= "";

        var entities = await (
            await _storage.QueryAsync<InviteCodeEntity>(x =>
                (active_only != true || x.Active == true) &&
                (platform == "" || x.Platform == platform))
        ).ToListAsync();

        var codes = entities.Select(x => x.ToModel());
        return new OkObjectResult(codes);
    }

    [Function("GetInviteCode_v1")]
    [OpenApiOperation(
        operationId: "getInviteCode",
        tags: ["Invites"],
        Summary = "Get invite code details")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiParameter("code", In = ParameterLocation.Path, Description = "Invite code identifier", Required = true)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(InviteCode), Description = "Information about invite code")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Authorization required")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid code format")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Code not found")]
    public async Task<IActionResult> GetInviteCode(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/invite-code/{code}")] HttpRequest req,
        FunctionContext context,
        string code)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.AdminInvites))
            return new ForbiddenResult();

        var valid = Guid.TryParse(code, out Guid guid);
        if (!valid)
            return new BadRequestResult();

        code = guid.ToString();
        var entity = await _storage.RetrieveAsync<InviteCodeEntity>(code);
        if (entity == null)
            return new NotFoundResult();

        var model = entity.ToModel();
        return new OkObjectResult(model);
    }

    [Function("CreateInviteCode_v1")]
    [OpenApiOperation(
        operationId: "createInviteCode",
        tags: ["Invites"],
        Summary = "Create invite codes")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiRequestBody("application/json", typeof(InviteCodeCreateRequest), Description = "Invite code creation request")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(InviteCode), Description = "Created invite code details")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Authorization required")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.UnprocessableEntity, Description = "Invalid request body")]
    public async Task<IActionResult> CreateInviteCode(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/invite-code")] HttpRequest req,
        FunctionContext context)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.AdminInvites))
            return new ForbiddenResult();

        var body = await req.ReadJson<InviteCodeCreateRequest>();
        if (body == null)
            return new BadRequestResult();

        var entity = new InviteCodeEntity()
        {
            Name = body.Name,
            Active = true,
            Created = DateTime.UtcNow,
            Expiry = DateTime.UtcNow.AddDays(body.ValidDays),
            OwnerId = auth.UserId,
            ClientId = body.ClientId,
            Limit = body.UseLimit,
            Used = 0,
            TrialDuration = body.TrialDuration,
            Platform = auth.Platform,
            AccountUpgrade = body.AllowAccountUpgrade
        };

        var ctx = req.HttpContext.Request;
        var uriBuilder = new UriBuilder()
        {
            Scheme = ctx.Scheme,
            Host = ctx.Host.Host,
            Port = ctx.Host.Port ?? -1,
            Path = $"{AireConstants.AppInvitePath}/{entity.Code()}"
        };
        entity.Link = uriBuilder.Uri.AbsoluteUri;

        {
            var stored = await _storage.UpsertAsync(entity);
            if (!stored)
            {
                _log.LogError("Failed to create new entity");
                return new StatusCodeResult((int)HttpStatusCode.FailedDependency);
            }
        }

        await _auditService.LogEvent(new(resType, entity.Code()), auth.UserId, "create");

        var model = entity.ToModel();
        return new OkObjectResult(model);
    }

    [Function("EditInviteCode_v1")]
    [OpenApiOperation(
        operationId: "editInviteCode",
        tags: ["Invites"],
        Summary = "Edit invite code")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiParameter("code", In = ParameterLocation.Path, Description = "Invite code identifier", Required = true)]
    [OpenApiRequestBody("application/json", typeof(InviteCode), Description = "Invite code")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(InviteCode), Description = "Invite code object")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Authorization required")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid code format or code mismatch")]
    [OpenApiResponseWithoutBody(HttpStatusCode.UnprocessableEntity, Description = "Invalid request body")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Code not found")]
    public async Task<IActionResult> EditInviteCode(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/v1/invite-code/{code}")] HttpRequest req,
        FunctionContext context,
        string code)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.AdminInvites))
            return new ForbiddenResult();

        var validCode = Guid.TryParse(code, out Guid guid);
        if (!validCode)
            return new BadRequestResult();

        code = guid.ToString();

        var body = await req.ReadJson<InviteCode>();
        if (body == null)
            return new UnprocessableEntityResult();

        var entity = await _storage.RetrieveAsync<InviteCodeEntity>(code);
        if (entity == null)
            return new NotFoundResult();

        if (body.Code != guid)
        {
            _log.LogError("Code mismatch");
            return new BadRequestResult();
        }

        entity.Name = body.Name;
        entity.Active = body.Active;
        entity.Limit = body.Limit;
        entity.Expiry = body.Expiry;
        entity.TrialDuration = body.TrialDuration;
        entity.AccountUpgrade = body.AccountUpgrade;

        {
            var updated = await _storage.UpsertAsync(entity);
            if (!updated)
            {
                _log.LogError("Failed to update entity");
                return new StatusCodeResult((int)HttpStatusCode.FailedDependency);
            }
        }

        await _auditService.LogEvent(new(resType, entity.Code()), auth.UserId, "edit");

        var model = entity.ToModel();
        return new OkObjectResult(model);
    }

    [Function("DeleteInviteCode_v1")]
    [OpenApiOperation(
        operationId: "deleteInviteCode",
        tags: ["Invites"],
        Summary = "Delete invite code")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiParameter("code", In = ParameterLocation.Path, Description = "Invite code identifier", Required = true)]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Invite code deleted")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Authorization required")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid code format")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Access denied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Code not found")]
    public async Task<IActionResult> DeleteInviteCode(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/v1/invite-code/{code}")] HttpRequest req,
        FunctionContext context,
        string code)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.AdminInvites))
            return new ForbiddenResult();

        var valid = Guid.TryParse(code, out Guid guid);
        if (!valid)
            return new BadRequestResult();

        code = guid.ToString();

        var entity = await _storage.RetrieveAsync<InviteCodeEntity>(code);
        if (entity == null)
            return new NotFoundResult();

        {
            var deleted = await _storage.DeleteAsync(entity);
            if (!deleted)
            {
                _log.LogError("Failed to delete entity");
                return new StatusCodeResult((int)HttpStatusCode.FailedDependency);
            }
        }

        await _auditService.LogEvent(new(resType, code), auth.UserId, "delete");

        return new NoContentResult();
    }

    [Function("SendInvitation_v1")]
    [OpenApiOperation(
        operationId: "sendInvitation",
        tags: ["Invites"],
        Summary = "Send invitation")]
    [OpenApiParameter("code", In = ParameterLocation.Path, Description = "Invite code identifier", Required = true)]
    [OpenApiRequestBody("application/json", typeof(Invitation), Description = "Invitation")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Invitation sent")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Conflict, Description = "Already has an existing user account or an active invite")]
    [OpenApiResponseWithoutBody(HttpStatusCode.UnprocessableEntity, Description = "Invalid request body")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Invite not found")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Expired invitation")]
    public async Task<IActionResult> SendInvitation(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/invite")] HttpRequest req)
    {
        var invitation = await req.ReadJson<Invitation>();
        if (invitation == null)
            return new UnprocessableEntityResult();

        var validCode = Guid.TryParse(invitation.Code, out Guid guid);
        if (!validCode)
            return new UnprocessableEntityResult();

        var validEmail = Validation.IsValidEmail(invitation.Email, true);
        if (!validEmail)
            return new UnprocessableEntityResult();

        var code = guid.ToString();

        // Check invitation code

        var inviteCode = await _storage.RetrieveAsync<InviteCodeEntity>(code);
        if (inviteCode == null)
            return new NotFoundResult();

        if (inviteCode.Expiry < DateTime.UtcNow || !inviteCode.Active || inviteCode.Used >= inviteCode.Limit)
            return new ForbiddenResult();

        // Validate target client

        var client = await _storage.RetrieveAsync<ClientEntity>(inviteCode.ClientId!);
        if (client == null)
        {
            _log.LogWarning("This invite is for a client that does not exist");
            return new ForbiddenResult();
        }

        var allowedPlatforms = client.GetAllowedPlatforms();
        if (allowedPlatforms.FirstOrDefault() != "*" && !allowedPlatforms.Contains(inviteCode.Platform!))
        {
            _log.LogWarning($"Client is not allowed to use platform {inviteCode.Platform}");
            return new ForbiddenResult();
        }

        // Check if email already registered / invited

        var emailHash = Crypto.SHA256Base16(invitation.Email!);

        // Find any active invitations
        var token = await (
            await _storage.QueryAsync<InviteTokenEntity>(x =>
                x.EmailHash == emailHash &&
                x.Expiry > DateTime.UtcNow &&
                x.Active)
        ).FirstOrDefaultAsync();

        if (token != null) // existing invite
        {
            // Allow re-sending invite mail if invite code matches
            if (token.Code != code)
                return new ConflictResult();
        }
        else // new invite
        {
            // Check whether an account already registered
            var user = await (
                await _storage.QueryAsync<UserEntity>(x => x.EmailHash == emailHash)
            ).FirstOrDefaultAsync();

            // Deny invite if user exists and is not an old trial user
            if (user != null && !user.HasRole(AireRoles.TrialUser))
                return new ConflictResult();

            token = new InviteTokenEntity()
            {
                Expiry = DateTime.UtcNow.AddDays(inviteCode.TrialDuration),
                EmailHash = emailHash,
                Active = true,
                UserId = null, // User entity is created when the user opens the chat for the first time
                ChatId = null, // Chat object is created on first activation
                ClientId = inviteCode.ClientId,
                Platform = inviteCode.Platform,
                Code = inviteCode.Code(),
                AccountUpgrade = inviteCode.AccountUpgrade,
            };

            {
                var created = await _storage.UpsertAsync(token);
                if (!created)
                {
                    _log.LogError("Failed to create invite token entity");
                    return new StatusCodeResult((int)HttpStatusCode.FailedDependency);
                }
            }

            // Update invite code

            inviteCode.Used += 1;
            {
                var updated = await _storage.UpsertAsync(inviteCode);
                if (!updated)
                {
                    _log.LogError("Failed to update invite code entity");
                    return new StatusCodeResult((int)HttpStatusCode.FailedDependency);
                }
            }
        }

        // Create invitation link and queue mail

        var invitationUrl = new UriBuilder(client.RedirectUri!)
        {
            Path = $"/invite/{token.Token()}",
            Query = $"?lang={invitation.Language}&platform={inviteCode.Platform}"
        }.Uri.AbsoluteUri;

        var mail = new MailTemplate
        {
            Locale = invitation.Language,
            Recipient = invitation.Email,
            TemplateName = MailTemplate.Invitation.Id,
            Values = new Dictionary<string, string> {
                { MailTemplate.Invitation.Params.Url, invitationUrl }
            }
        };

        {
            var result = await _mailQueue.SendMessageAsync(mail.ObjectToJson());
            _log.LogInformation($"Queued invitation email. MessageId: {result.Value.MessageId}");
        }

        await _auditService.LogEvent(new(resType, token.Code), inviteCode.OwnerId ?? "", "invitation.send", new()
        {
            { "inviteToken", token.Token() },
            { "emailHash", emailHash }
        });

        return new NoContentResult();
    }

    [Function("ValidateInvite_v1")]
    [OpenApiOperation(
        operationId: "validateInvite",
        tags: ["Invites"],
        Summary = "Validate and authenticate with invite token")]
    [OpenApiParameter("token", In = ParameterLocation.Path, Description = "Invite token", Required = true)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(InviteAuthResponse), Description = "Token response")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Authorization required")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Invalid token")]
    public async Task<IActionResult> ValidateInvite(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/invite/{token}")] HttpRequest req,
        string token)
    {
        // Validate invite token
        var valid = Guid.TryParse(token, out Guid guid);
        if (!valid)
            return new ForbiddenResult();

        token = guid.ToString();
        var entity = await _storage.RetrieveAsync<InviteTokenEntity>(token[..5], token);
        if (entity?.Platform == null)
            return new ForbiddenResult();

        // Invitation code has to exist (it's okay if it is disabled or expired)
        var inviteCode = await _storage.RetrieveAsync<InviteCodeEntity>(entity.Code!);
        if (inviteCode == null)
            return new ForbiddenResult();

        bool expired = entity.Expiry < DateTime.UtcNow || !entity.Active;

        if (expired)
            return new ForbiddenResult();

        // Create new trial user
        UserEntity? user;
        if (entity.UserId == null)
        {
            user = UserEntity.CreateTrialUser(entity.EmailHash!, entity.Token(), entity.Platform);
            var created = await _storage.UpsertAsync(user);
            if (!created)
            {
                _log.LogError("Failed to create user");
                return new StatusCodeResult((int)HttpStatusCode.FailedDependency);
            }

            entity.UserId = user.UUID();
            var updated = await _storage.UpsertAsync(entity);
            if (!updated)
            {
                _log.LogError("Failed to update invite token");
                return new StatusCodeResult((int)HttpStatusCode.FailedDependency);
            }

            await _auditService.LogEvent(new(resType, entity.Code), entity.UserId, "invitation.activate", new()
            {
                { "inviteToken", entity.Token() },
                { "emailHash", entity.EmailHash }
            });
        }
        else
        {
            user = await _storage.RetrieveAsync<UserEntity>(entity.UserId);
        }

        if (user == null)
        {
            _log.LogWarning("User does not exist anymore");
            return new ForbiddenResult(); // User removed, no longer valid
        }

        if (!user.HasRole(AireRoles.TrialUser))
        {
            _log.LogWarning("User is not an trial user anymore");
            return new ForbiddenResult(); // User removed, no longer valid
        }

        // Check for old trial user roles and move the role to the new one
        var rights = user.GetAccessRights();
        var currentTrial = rights.FirstOrDefault(x => x.Value.Role == AireRoles.TrialUser);
        if (currentTrial.Key != entity.Platform)
        {
            user.ClearAccessRights(entity.Platform);
            user.SetRole(entity.Platform, AireRoles.TrialUser);
            
            var updated = await _storage.UpsertAsync(user);
            if (!updated)
            {
                _log.LogError("Failed to update user");
                return new StatusCodeResult((int)HttpStatusCode.FailedDependency);
            }
        }

        var trialpass = user.GetTrialUserPassword(user.EmailHash!, token);
        var key = user.GetEncryptionKey(trialpass);
        var subject = _loginProvider.GetSubject(user, key!, entity.Platform);
        subject.Claims.Add(AireClaims.Platform, entity.Platform);

        // Create new chat object
        if (entity.ChatId == null)
        {
            var memory = await _platformService.GetPlatformModule(entity.Platform!, ModuleType.Memory, null);
            if (memory == null)
            {
                _log.LogError("Memory service unavailable");
                return new StatusCodeResult((int)HttpStatusCode.FailedDependency);
            }

            var tokenDesc = new OauthTokenDescription(subject, [AireScopes.WriteChatHistory], TimeSpan.FromMinutes(5));
            var clientToken = _tokenProvider.IssueNewToken(tokenDesc);
            var memoryClient = await _clientFactory.CreateMemoryClient(memory, clientToken);

            var metadata = await memoryClient.CreateChatlog();
            if (metadata == null)
            {
                _log.LogError("Failed to create new chatlog");
                return new StatusCodeResult((int)HttpStatusCode.FailedDependency);
            }

            entity.ChatId = metadata.Id;
            var updated = await _storage.UpsertAsync(entity);
            if (!updated)
            {
                _log.LogError("Failed to update invite token");
                return new StatusCodeResult((int)HttpStatusCode.FailedDependency);
            }
        }

        // Create access token

        var scopes = user.GetScopes(entity.Platform);
        if (!entity.AccountUpgrade)
            scopes.Remove(AireScopes.TrialAccountUpgrade);

        var client = await _storage.RetrieveAsync<ClientEntity>(entity.ClientId!);
        if (client == null)
            return new ForbiddenResult();

        var validScopes = OauthAuthenticationService.ValidateClientScopes(scopes.ToString(), client);
        scopes = [.. validScopes!];

        var expiresIn = TimeSpan.FromHours(2);
        var tokenDescriptor = new OauthTokenDescription(subject, scopes, expiresIn);
        var accessToken = _tokenProvider.IssueNewToken(tokenDescriptor);

        // Response

        var response = new InviteAuthResponse()
        {
            Auth = new OauthTokenResponse
            {
                AccessToken = accessToken,
                Scope = scopes.ToString(),
                ExpiresIn = (int)expiresIn.TotalSeconds,
                TokenType = OauthTokenType.Bearer
            },
            AccountUpgrade = entity.AccountUpgrade,
            ChatId = entity.ChatId,
            UserId = entity.UserId,
            Platform = entity.Platform,
            Invitation = inviteCode.Name
        };

        await _auditService.LogEvent(new(resType, entity.Code), entity.UserId, "invitation.validate", new()
        {
            { "inviteToken", entity.Token() },
            { "emailHash", entity.EmailHash }
        });

        return new OkObjectResult(response);
    }

    [Function("SignupWithInvite_v1")]
    [OpenApiOperation(
        operationId: "signupWithInvite",
        tags: ["Invites"],
        Summary = "Sign-up invited user")]
    [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT", Description = "User token")]
    [OpenApiParameter("token", In = ParameterLocation.Path, Description = "Invite token", Required = true)]
    [OpenApiRequestBody("application/json", typeof(InviteAccountUpgradeRequest), Description = "Signup request", Required = true)]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Signup success")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Authorization required")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid request")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Forbidden, Description = "Invalid token or account upgrade disabled")]
    [OpenApiResponseWithoutBody(HttpStatusCode.UnprocessableEntity, Description = "Invalid request body")]
    public async Task<IActionResult> SignupWithInvite_v1(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/invite/{token}/signup")] HttpRequest req,
        FunctionContext context,
        string token)
    {
        var auth = context.Features.Get<JwtAuthFeature>();
        if (auth?.Platform == null)
            return new UnauthorizedResult();

        if (!_jwt.CheckAuthorization(auth, requiredScopes: AireScopes.TrialAccountUpgrade))
            return new ForbiddenResult();

        var body = await req.ReadJson<InviteAccountUpgradeRequest>();
        if (body?.Password == null)
            return new UnprocessableEntityResult();

        var validPassword = Validation.IsValidPassword(body.Password);
        if (!validPassword)
            return new BadRequestResult();

        // Validate token

        var validGuid = Guid.TryParse(token, out Guid guid);
        if (!validGuid)
            return new BadRequestResult();
        token = guid.ToString();

        var tokenEntity = await _storage.RetrieveAsync<InviteTokenEntity>(token[..5], token);
        if (tokenEntity == null)
            return new ForbiddenResult();

        // Upgrade account to user role

        var user = await _storage.RetrieveAsync<UserEntity>(auth.UserId);
        if (user == null)
            return new ForbiddenResult();

        if (!user.HasRole(AireRoles.TrialUser, auth.Platform))
        {
            _log.LogWarning("Already signed up");
            return new NoContentResult();
        }

        bool upgraded = user.UpgradeTrialUserToRegular(body.Password, token, auth.Platform);
        if (!upgraded)
        {
            _log.LogError("Failed to upgrade account");
            return new BadRequestResult();
        }

        {
            var updated = await _storage.UpsertAsync(user);
            if (!updated)
            {
                _log.LogError("Failed to update user entity");
                return new StatusCodeResult((int)HttpStatusCode.FailedDependency);
            }
        }

        // Deactivate invite token

        {
            var deleted = await _storage.DeleteAsync(tokenEntity);
            if (!deleted)
                _log.LogError($"Failed to delete invite token '{token}'");
        }

        await _auditService.LogEvent(new(resType, tokenEntity.Code), auth.UserId, "invitation.signup", new()
        {
            { "inviteToken", tokenEntity.Token() },
            { "emailHash", tokenEntity.EmailHash }
        });

        return new NoContentResult();
    }
}