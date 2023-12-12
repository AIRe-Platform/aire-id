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
        [OpenApiOperation(operationId: "Run", tags: new[] { "User sign-up and verification" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Verification succeeded")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Verification failed")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/verify")] HttpRequest req)
        {
            // TODO: Implement verification

            return await Task.FromResult(new NotFoundResult());
        }
    }
}

