# AIRe ID

This module handles authentication and user accounts.

## Getting Started

Open the solution in VS Code (on Windows/Linux/macOS). Install the recommended extensions. Hit F5 and you should be good to go.

You may also use Visual Studio on macOS and Windows.

Remember to configure!

## Configuration

You should create `local.settings.json` in the root of the repository when developing locally. It should look something like this:

```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "",
        "StorageConnectionString": "<Connection string for Table storage or storage emulator>",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "AirePlatformService": "http://localhost:7071/api",
        "TokenSigningKey": "<signing key shared between platform modules>",
        "TokenEncryptionKey": "<enryption key shared between platform modules>",
        "TokenIssuer": "http://localhost:7072",
        "TokenAudience": "http://localhost",
        "OpenApi__HostNames": "http://localhost:7072/api/"
    },
    "Host": {
        "LocalHttpPort": 7072,
        "CORS": "*",
        "CORSCredentials": false
    }
}
```
## API Documentation

Visit path `/api/swagger/ui` to inspect. If running in localhost, there's an issue where the configuration file URL gets an invalid port.

Simply change in the correct port in the top bar to work around the issue.

Example: If the module is running on port `7072` change the URL to `http://localhost:7072/api/swagger.json`.

## Deployment

Publish the Functions app and then setup the following required environment values:

- `StorageConnectionString` Azure Table Storage connection string
- `AirePlatformService` The endpoint of the AIRe Services module.
- `TokenSigningKey` The token signing key shared between the platform instance modules.
- `TokenEncryptionKey` The token encryption key shared between the platform instance modules.
- `TokenIssuer` The token issuer host URL, the same as the service's host.
- `TokenAudience` Comma-separated list of audiences.

## Disclaimer

This README is a work-in-progress. The information above may be out-dated or incorrect.
