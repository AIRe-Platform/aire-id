# AIRe ID

This module handles authentication and user accounts.

## Getting Started

You need to have [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed. Pull the repository and its submodules.

Open the solution in VS Code (recommended, works on Windows/Linux/macOS). You may also use Visual Studio on macOS and Windows.

On VS Code: Install the recommended extensions.

Configure `local.settings.json` as instructed. You should be running the AIRe Services module locally with Azurite. Add `mail-queue` queue into the Table Storage if it does not exist.

Hit F5 and you should be good to go.

## Configuration

You should create `local.settings.json` in the root of the repository when developing locally. It should look something like this:

```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "",
        "StorageConnectionString": "<Connection string for Table storage or storage emulator>",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "AIRE_SERVICE_BASE": "http://localhost:7071/api",
        "AIRE_SERVICE_KEY": "<service key secret>",
        "TOKEN_ENCRYPTION_KEY": "<enryption key shared between platform modules>",
        "TOKEN_SIGNING_KEY": "<signing key shared between platform modules>",
        "TokenIssuer": "http://localhost:7072",
        "TokenAudience": "http://localhost",
    },
    "Host": {
        "LocalHttpPort": 7072,
        "CORS": "*",
        "CORSCredentials": false
    }
}
```

Use the same token keys you are using in AIRe Services module.

## API Documentation

Visit path `/api/swagger/ui` to inspect. The default host is set to `/api` path.

You can set a custom host with `OpenApi__HostNames` environment value.

## Deployment

Publish the Functions app and then setup the following required environment values:

- `StorageConnectionString` Azure Table Storage connection string
- `COMMUNICATION_SERVICES_CONNECTION_STRING`: Azure Communication Services (ACS) connection string.
- `AIRE_SERVICE_BASE` The endpoint of the AIRe Services module.
- `AIRE_SERVICE_KEY` The service key for the AIRe Services module.
- `TOKEN_SIGNING_KEY` The token signing key shared between the platform instance modules.
- `TOKEN_ENCRYPTION_KEY` The token encryption key shared between the platform instance modules.
- `TokenIssuer` The token issuer host URL, the same as the service's host.
- `TokenAudience` Comma-separated list of audiences. (Optional)
- `EmailDomainWhitelist` Comma-separated list of allowed email domains. Do not set to allow all domains.
- `EmailSenderAddress` The sender address configured in ACS.

## Disclaimer

This README is a work-in-progress. The information above may be out-dated or incorrect.
