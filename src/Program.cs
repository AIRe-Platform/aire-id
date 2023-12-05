using System;
using Aire.Helpers;
using Aire.Id.Models;
using Aire.Id.Oauth2;
using Aire.Id.Oauth2.Providers;
using Aire.Id.Providers;
using Aire.Id.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker => worker.UseNewtonsoftJson())
    .ConfigureOpenApi()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddHttpContextAccessor();
        services.AddSingleton<ITableStorageService, TableStorageService>()
                .Configure<OauthConfiguration>(o => {
                    o.Roles = new() { 
                        AireConstants.Roles.User, 
                        AireConstants.Roles.Admin
                    };
                    o.Scopes = new() { 
                        AireConstants.Scopes.ReadProfile,
                        AireConstants.Scopes.EditProfile,
                        AireConstants.Scopes.DeleteProfile,
                    };
                    o.DefaultRoleScopes = new() {
                        { 
                            AireConstants.Roles.User, new() { 
                                AireConstants.Scopes.ReadProfile,
                                AireConstants.Scopes.EditProfile,
                                AireConstants.Scopes.DeleteProfile
                            }
                        }
                    };
                    o.TokenLifetime = TimeSpan.FromDays(3);
                    o.DefaultScopes = new() {};
                });


        services.AddSingleton<IOauthTokenProvider, TokenProvider>();
        services.AddSingleton<IOauthLoginProvider, LoginProvider>();
        services.AddSingleton<OauthAuthenticationService>();
    })
    .UseJwtTokenBinding()
    .Build();

host.Run();
