using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Aire.Id.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Aire.Helpers
{
    public static class JwtAuthExtension
    {
        public static IFunctionsWorkerApplicationBuilder UseJwtAuth(this IFunctionsWorkerApplicationBuilder builder)
        {   
            builder.Services
                .AddSingleton<JwtSecurityTokenHandler>()
                .AddSingleton<IJwtTokenService, JwtTokenService>();

            builder.UseMiddleware<JwtAuthMiddleware>(); 

            return builder;
        }
    }

    public class JwtAuthFeature
    {
        public ClaimsPrincipal Principal { get; set; }
        public JwtSecurityToken Token { get; set; }
    }

    public class JwtAuthMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly JwtSecurityTokenHandler _handler;
        private readonly TokenValidationParameters _validationParams;
        private readonly ILogger<JwtAuthMiddleware> _log;

        public JwtAuthMiddleware(
            JwtSecurityTokenHandler handler, TokenValidationParameters validationParams, ILogger<JwtAuthMiddleware> log)
        {
            _handler = handler;
            _validationParams = validationParams;
            _log = log;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var httpContext = context.GetHttpContext();
            var tokenString = ParseAuthHeader(httpContext);
            if(!string.IsNullOrWhiteSpace(tokenString))
            {
                try
                {
                    ClaimsPrincipal principal = _handler.ValidateToken(tokenString, _validationParams, out var jwt);
                    var token = (JwtSecurityToken) jwt;

                    context.Features.Set(new JwtAuthFeature{
                        Principal = principal,
                        Token = token
                    });
                }
                catch(Exception ex)
                {
                    _log.LogWarning(ex, "Token validation failed");
                }
            }

            await next(context);
        }

        private string ParseAuthHeader(HttpContext http)
        {
            if(http == null) return null;
            if(http.Request == null) return null;

            IHeaderDictionary headers = http.Request.Headers;
            if(!headers.TryGetValue("Authorization", out var value))
            {
                _log.LogWarning("Missing Authorization header");
                return null;
            }

            var parts = ((string)value).Split(" ");
            if(parts[0] != "Bearer")
            {
                _log.LogWarning("Invalid Authorization scheme");
                return null;
            }

            if(parts.Length != 2)
            {
                _log.LogWarning("Invalid Authorization header format");
                return null;
            }

            return parts[1];
        }
    }
}