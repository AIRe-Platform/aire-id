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
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "SERVICES_ENDPOINT": "http://localhost:7071/api"
  },
  "Host": {
    "LocalHttpPort": 7072,
    "CORS": "*",
    "CORSCredentials": false
  }
}
```

## Deployment

Publish the Functions app and then setup the following required environment values:

- Storage connection string: `StorageConnectionString` (note: probably the same as your `AzureWebJobsStorage`)
- AIRe Services endpoint: `SERVICES_ENDPOINT`
