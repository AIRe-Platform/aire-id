using System.Net;
using Aire.Sdk.TableStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Aire.Id
{
    public class Verify_v1
    {
        private readonly ITableStorageService _storage;
        private readonly ILogger<Verify_v1> _log;

        public Verify_v1(ITableStorageService storage, ILogger<Verify_v1> log)
        {
            _storage = storage;
            _log = log;
        }

        [Function("Verify_v1")]
        [OpenApiOperation(operationId: "Run", tags: ["User sign-up and verification"], Description = "Verify user")]
        [OpenApiParameter("code", Description = "Verification code")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Verification succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Verification failed")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotImplemented, Description = "This endpoint is not yet implemented!")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/verify/{code}")] HttpRequest req,
            string code)
        {
            // TODO: Implement verification
            _log.LogInformation($"Verification code: {code}");
            _log.LogError("Not implemented!");
            return await Task.FromResult(
                new StatusCodeResult((int) HttpStatusCode.NotImplemented)
            );
        }
    }
}

