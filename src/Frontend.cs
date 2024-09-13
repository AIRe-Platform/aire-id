using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace Aire.Id;

public class Frontend
{
    private readonly ILogger<Frontend> _log;

    public Frontend(ILogger<Frontend> log)
    {
        _log = log;
    }

    private IActionResult ServeStaticFile(string filePath)
    {
        _log.LogInformation("Request static file: {filePath}", filePath);

        var root = Path.Combine(Environment.CurrentDirectory, "www");
        var path = Path.Combine(root, filePath);
        var relative = Path.GetRelativePath(root, path);

        if (relative.StartsWith('.'))
        {
            _log.LogWarning("Not a valid path: {filePath}", relative);
            return new NotFoundResult();
        }

        if (!File.Exists(path))
        {
            _log.LogError("File not found: {filePath}", path);
            return new NotFoundResult();
        }

        var stream = File.OpenRead(path);
        if (!stream.CanRead)
        {
            _log.LogError("Cannot read file: {filePath}", path);
            return new InternalServerErrorResult();
        }

        var mime = MimeTypes.MimeTypeMap.GetMimeType(path);
        return new FileStreamResult(stream, mime);
    }

    [Function("Assets")]
    [OpenApiIgnore]
    public IActionResult GetStaticFile(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "assets/{*file}")] HttpRequest req,
        string? file)
    {
        if (string.IsNullOrEmpty(file))
            return new NotFoundResult();

        var path = Path.Combine("assets", file);
        return ServeStaticFile(path);
    }

    [Function("FrontendApplication")]
    [OpenApiIgnore]
    public IActionResult FrontendApplication(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "app/{*path}")] HttpRequest req,
        string? path)
    {
        return ServeStaticFile("index.html");
    }
}
