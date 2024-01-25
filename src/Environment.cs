namespace Aire
{
    public static class AireEnvironment
    {
        public static string? StorageConnectionString => Environment.GetEnvironmentVariable("StorageConnectionString");
        public static string? CommunicationServiceConnectionString => Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING");
        public static string? EmailDomainWhitelist => Environment.GetEnvironmentVariable("EmailDomainWhitelist");
        public static string? EmailSenderAddress => Environment.GetEnvironmentVariable("EmailSenderAddress");
        public static string? AirePlatformService => Environment.GetEnvironmentVariable("AirePlatformService");
        public static string? TokenSigningKey => Environment.GetEnvironmentVariable("TokenSigningKey");
        public static string? TokenEncryptionKey => Environment.GetEnvironmentVariable("TokenEncryptionKey");
        public static string? TokenIssuer => Environment.GetEnvironmentVariable("TokenIssuer");
        public static string? TokenAudience => Environment.GetEnvironmentVariable("TokenAudience");
        public static string? OpenApiHost => Environment.GetEnvironmentVariable("OpenApi__HostNames");
    }
}