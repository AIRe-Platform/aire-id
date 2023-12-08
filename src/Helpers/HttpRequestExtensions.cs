using Microsoft.AspNetCore.Http;

namespace Aire.Helpers
{
    public static class HttpRequestExtensions
    {
        public static async Task<T?> ReadJson<T>(this HttpRequest req)
        {
            if(req.ContentType == "application/json" && req.Body != null)
            {
                try
                {
                    var reader = new StreamReader(req.Body);
                    string body = await reader.ReadToEndAsync();
                    return body.JsonToObject<T>();
                }
                catch(Exception) {}
            }
            return default;
        }

        public static string? ReadParam(this HttpRequest req, string param)
        {
            string? value = req.Form[param];
            value ??= req.Query[param];
            return value;
        }
    }
}