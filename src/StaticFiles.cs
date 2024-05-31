using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace Aire.Id;

public class StaticFiles
{
    private readonly ILogger<StaticFiles> _log;

    public StaticFiles(ILogger<StaticFiles> log)
    {
        _log = log;
    }

    [Function("StaticFiles")]
    [OpenApiIgnore]
    public IActionResult GetStaticFile(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "static/{*file}")] HttpRequest req,
        string? file)
    {
        if(string.IsNullOrEmpty(file))
            return new NotFoundResult();

        var staticFiles = Path.Combine(Environment.CurrentDirectory, "www/static/");
        var path = Path.Combine(staticFiles, file);

        _log.LogInformation("Request static file: {filePath}", path);

        var relative = Path.GetRelativePath(staticFiles, path);
        if(relative.StartsWith('.'))
        {
            _log.LogWarning("Not a valid path: {filePath}", relative);
            return new NotFoundResult();
        }

        if(!File.Exists(path))
            return new NotFoundResult();

        var stream = File.OpenRead(path);
        if(!stream.CanRead)
        {
            _log.LogError("Cannot read file: {filePath}", path);
            return new InternalServerErrorResult();
        }

        var mime = MimeTypes.MimeTypeMap.GetMimeType(path);
        return new FileStreamResult(stream, mime);
    }
}
