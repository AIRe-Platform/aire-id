using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Aire.Helpers
{
    public class ForbiddenResult : StatusCodeResult
    {
        public ForbiddenResult() : base((int) HttpStatusCode.Forbidden) {}
    }

    public class ForbiddenObjectResult : ObjectResult
    {
        public ForbiddenObjectResult(object obj) : base(obj)
        {
            StatusCode = (int) HttpStatusCode.Forbidden;
        }
    }
}