using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Aire.Helpers;
using Aire;
using Aire.Id.Oauth2;
using Aire.Id.Providers;
using Aire.Id.Services;
using Aire.Sdk.Auth.Extensions;
using Newtonsoft.Json;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Aire.Sdk.Auth.Roles;
using Aire.Sdk.Auth.Scopes;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker => {
        worker.UseNewtonsoftJson();
        worker.UseJwtAuth();
        worker.UseOauth<TokenProvider, LoginProvider>();
    })
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddMvcCore().AddNewtonsoftJson(options => {
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        });

        services
            .AddSingleton<ITableStorageService, TableStorageService>()
            .Configure<OauthConfiguration>(o => {
                o.DefaultRoleScopes = new() {
                    { AireRoles.User, AireScopes.UserScopes }
                };
                o.TokenLifetime = TimeSpan.FromDays(3);
                o.DefaultScopes = [];
            });

        var signingKeyBytes = Encoding.ASCII.GetBytes(AireEnvironment.TokenSigningKey);
        var decryptionKeyBytes = Encoding.ASCII.GetBytes(AireEnvironment.TokenEncryptionKey);

        services.AddSingleton(_ => {
            var validationParams = new TokenValidationParameters {
                RequireSignedTokens = true,
                RequireAudience = true,
                RequireExpirationTime = true,

                ValidAudience = AireEnvironment.TokenAudience,
                ValidIssuer = AireEnvironment.TokenIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes),
                TokenDecryptionKey = new SymmetricSecurityKey(decryptionKeyBytes),

                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true
            };
            return validationParams;
        });
        
        services.AddSingleton<IOpenApiConfigurationOptions>(_ => {
            var options = new OpenApiConfigurationOptions {
                Info = new OpenApiInfo {
                    Version = "0.1.0",
                    Title = "AIRe ID Module",
                    Description = "This is the reference implementation of AIRe Platform ID module."
                },
                Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
                OpenApiVersion = OpenApiVersionType.V3,
                IncludeRequestingHostName = true,
                ForceHttp = false,
                ForceHttps = false,
            };
            return options;
        });
    })
    .Build();

host.Run();
