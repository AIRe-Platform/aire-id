using System;
using Aire.Helpers;
using Aire.Id.Models;
using Aire.Id.Oauth2;
using Aire.Id.Oauth2.Providers;
using Aire.Id.Providers;
using Aire.Id.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(Aire.Id.Startup))]

namespace Aire.Id
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.UseJwtTokenBinding();

            builder.Services
                .AddHttpContextAccessor()
                .AddSingleton<ITableStorageService, TableStorageService>()
                .Configure<OauthConfiguration>(o => {
                    o.Roles = new() { UserRoles.User, UserRoles.Admin };
                    o.Scopes = new() {};
                    o.DefaultRoleScopes = new() {};
                    o.TokenLifetime = TimeSpan.FromDays(3);
                    o.DefaultScopes = new() {};
                })
                .AddSingleton<IOauthTokenProvider, TokenProvider>()
                .AddSingleton<IOauthLoginProvider, LoginProvider>()
                .AddSingleton<OauthAuthenticationService>()
                .AddLogging();
        }
    }
}
