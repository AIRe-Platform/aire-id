using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Aire.Helpers;
using Aire.Id.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

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

        [FunctionName("Verify_v1")]
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

