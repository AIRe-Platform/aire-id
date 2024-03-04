using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Aire;
using Aire.Id.Oauth2;
using Aire.Id.Providers;
using Aire.Sdk.Azure;
using Aire.Sdk.Auth.Extensions;
using Aire.Sdk.Auth.Roles;
using Aire.Sdk.Auth.Scopes;
using Aire.Sdk.Auth.Models;
using Azure.Storage.Queues;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker => {
        worker.UseNewtonsoftJson();
        worker.UseJwtAuth(new JwtTokenServiceConfiguration() {
            Issuer = AireEnvironment.TokenIssuer,
            Audience = AireEnvironment.TokenAudience,
            SigningKey = AireEnvironment.TokenSigningKey,
            EncryptionKey = AireEnvironment.TokenEncryptionKey
        });
        worker.UseOauth<TokenProvider, LoginProvider>();
    })
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddMvcCore().AddNewtonsoftJson(options => {
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        });

        services.AddAzureClients(builder => {
            builder
                .AddEmailClient(AireEnvironment.CommunicationServiceConnectionString)
                .WithName("acs");

            builder.AddQueueServiceClient(AireEnvironment.StorageConnectionString)
                .ConfigureOptions(options => {
                    options.MessageEncoding = QueueMessageEncoding.Base64;
                })
                .WithName("queue-client");
        });

        services
            .AddSingleton<ITableStorageService, TableStorageService>()
            .Configure<OauthConfiguration>(o => {
                o.DefaultRoleScopes = AireScopes.DefaultRoleScopes;
                o.TokenLifetime = TimeSpan.FromDays(3);
                o.DefaultScopes = [];
            })
            .Configure<TableStorageConfiguration>(o => {
                o.ConnectionString = AireEnvironment.StorageConnectionString;
            });
        
        services.AddSingleton<IOpenApiConfigurationOptions>(_ => {
            var options = new OpenApiConfigurationOptions {
                Info = new OpenApiInfo {
                    Version = "0.1.0",
                    Title = "AIRe ID Module",
                    Description = "This is the reference implementation of AIRe Platform ID module."
                },                
                Servers = [
                    new OpenApiServer { Url = AireEnvironment.OpenApiHost ?? "/api" }
                ],
                OpenApiVersion = OpenApiVersionType.V3,
                IncludeRequestingHostName = false,
                ForceHttp = false,
                ForceHttps = false,
            };
            return options;
        });
    })
    .Build();

host.Run();
