namespace Aire
{
    public static class AireEnvironment
    {
        public static string? StorageConnectionString => Environment.GetEnvironmentVariable("StorageConnectionString");
        public static string? CommunicationServiceConnectionString => Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING");
        public static string? EmailDomainWhitelist => Environment.GetEnvironmentVariable("EmailDomainWhitelist");
        public static string? EmailSenderAddress => Environment.GetEnvironmentVariable("EmailSenderAddress");
        public static string? TokenSigningKey => Environment.GetEnvironmentVariable("TOKEN_SIGNING_KEY");
        public static string? TokenEncryptionKey => Environment.GetEnvironmentVariable("TOKEN_ENCRYPTION_KEY");
        public static string? PlatformServiceKey => Environment.GetEnvironmentVariable("AIRE_SERVICE_KEY");
        public static string? PlatformServiceUrl => Environment.GetEnvironmentVariable("AIRE_SERVICE_BASE");
        public static string? TokenIssuer => Environment.GetEnvironmentVariable("TOKEN_ISSUER");
        public static string? TokenAudience => Environment.GetEnvironmentVariable("TOKEN_AUDIENSE");
        public static string? AuthLoginRedirect => Environment.GetEnvironmentVariable("AUTH_LOGIN_REDIRECT");
        public static string? GlobalRecoveryKey => Environment.GetEnvironmentVariable("GLOBAL_RECOVERY_KEY");
        public static string? OpenApiHost => Environment.GetEnvironmentVariable("OpenApi__HostNames");
    }
}