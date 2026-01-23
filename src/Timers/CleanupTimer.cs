// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Id.Models;
using Aire.Id.Oauth2.Models;
using Aire.Id.Oauth2.Providers;
using Aire.Sdk.Auth;
using Aire.Sdk.Azure;
using Aire.Sdk.Models.Platform;
using Aire.Sdk.Platform;
using Aire.Sdk.Platform.Clients;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Aire.Id.Timers
{
    public class CleanupTimer(
        ILoggerFactory loggerFactory, ITableStorageService storage,
        IAirePlatformService platformService, IAireClientFactory clientFactory,
        IOauthLoginProvider loginProvider, IOauthTokenProvider tokenProvider)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<CleanupTimer>();
        private readonly ITableStorageService _storage = storage;
        private readonly IAirePlatformService _platform = platformService;
        private readonly IAireClientFactory _clientFactory = clientFactory;
        private readonly IOauthLoginProvider _loginProvider = loginProvider;
        private readonly IOauthTokenProvider _tokenProvider = tokenProvider;

        [Function("CleanupTimer")]
        public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo timer)
        {
            _logger.LogInformation($"Running clean up at: {DateTime.UtcNow}");

            {
                _logger.LogInformation("Deleting expired auth codes");
                int count = 0;
                int errors = 0;
                var query = await _storage.QueryAsync<AuthCodeEntity>(x => x.Expires < DateTime.UtcNow);

                await foreach (var item in query)
                {
                    var res = await _storage.DeleteAsync(item);
                    if (res)
                    {
                        count++;
                    }
                    else
                    {
                        errors++;
                        _logger.LogError("Failed to delete auth code '{code}'", item.RowKey);
                    }
                }

                _logger.LogInformation("Finished cleaning up expired auth codes.\nDeleted: {count}\nFailures: {errors}", count, errors);
            }

            {
                _logger.LogInformation("Deleting expired invites and trial users");
                var query = await _storage.QueryAsync<InviteTokenEntity>(x => x.Expiry < DateTime.UtcNow);

                await foreach (var item in query)
                {
                    await _storage.DeleteAsync(item);

                    if (item.UserId == null)
                        continue;

                    var user = await _storage.RetrieveAsync<UserEntity>(item.UserId);
                    if (user == null || user.Role != AireRoles.TrialUser) // Deleted or upgraded?
                        continue;

                    _logger.LogInformation($"Deleting user {user.UUID()}");

                    var platform = await _platform.GetPlatformConfiguration(item.Platform!);
                    if (platform == null)
                    {
                        _logger.LogWarning($"Platform config not found, will not clean user data!");
                        continue;
                    }

                    var pw = user.GetTrialUserPassword(item.EmailHash!, item.Token());
                    var key = user.GetEncryptionKey(pw);

                    var subject = _loginProvider.GetSubject(user, key!);
                    subject.Claims[AireClaims.Platform] = item.Platform!;

                    var tokendesc = new OauthTokenDescription(subject, [AireScopes.DeleteChatHistory], TimeSpan.FromMinutes(5));
                    var jwt = _tokenProvider.IssueNewToken(tokendesc);

                    var memories = platform.GetModules(ModuleType.Memory, false);
                    foreach (var memory in memories)
                    {
                        var memoryService = await _clientFactory.CreateMemoryClient(memory, jwt);
                        if (memoryService != null)
                        {
                            bool dataDeleted = await memoryService.DeleteUserData(false);
                            if (!dataDeleted)
                                _logger.LogError("Failure to delete data from the Memory module {id}", memory.Id);
                        }
                    }
                }
            }

            _logger.LogInformation($"Cleanup finished at: {DateTime.UtcNow}");
        }
    }
}
