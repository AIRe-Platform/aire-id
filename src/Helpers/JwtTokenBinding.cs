using System;
using System.IdentityModel.Tokens.Jwt;
using Aire.Id.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.DependencyInjection;

namespace Aire.Helpers
{
    public static class JwtTokenBindingExtension
    {
        public static IWebJobsBuilder UseJwtTokenBinding(this IWebJobsBuilder builder)
        {
            if(builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.AddExtension<JwtTokenBinding>();

            builder.Services
                .AddSingleton<JwtSecurityTokenHandler>()
                .AddSingleton<IJwtTokenService, JwtTokenService>();

            return builder;
        }
    }

    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class JwtTokenAttribute : Attribute
    {
        // Comma-separated list of roles
        public string AllowRoles { get; set; }

        // Comma-separated list of scopes
        public string RequireScopes { get; set; }

        public JwtTokenAttribute() {}
    }

    public class JwtTokenBinding : IExtensionConfigProvider
    {
        private readonly IJwtTokenService _tokenService;

        public JwtTokenBinding(IJwtTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            var rule = context.AddBindingRule<JwtTokenAttribute>();
            rule.BindToInput(GetTokenAsync);
        }

        private JwtSecurityToken GetTokenAsync(JwtTokenAttribute attribute)
        {
            return _tokenService.ValidateRequestToken(attribute.AllowRoles, attribute.RequireScopes);
        }
    }
}