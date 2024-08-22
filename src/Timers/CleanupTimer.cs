// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Id.Models;
using Aire.Sdk.Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Aire.Id.Timers
{
    public class CleanupTimer
    {
        private readonly ILogger _logger;
        private readonly ITableStorageService _storage;

        public CleanupTimer(ILoggerFactory loggerFactory, ITableStorageService storage)
        {
            _logger = loggerFactory.CreateLogger<CleanupTimer>();
            _storage = storage;
        }

        [Function("CleanupTimer")]
        public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo _)
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

            _logger.LogInformation($"Cleanup finished at: {DateTime.UtcNow}");
        }
    }
}
