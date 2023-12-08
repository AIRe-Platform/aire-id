namespace Aire
{
    public static class AireEnvironment
    {
        public static string? StorageConnectionString => Environment.GetEnvironmentVariable("StorageConnectionString");

        public static string? AirePlatformService => Environment.GetEnvironmentVariable("AirePlatformService");

        public static string? TokenSigningKey => Environment.GetEnvironmentVariable("TokenSigningKey");

        public static string? TokenEncryptionKey => Environment.GetEnvironmentVariable("TokenEncryptionKey");
        
        public static string? TokenIssuer => Environment.GetEnvironmentVariable("TokenIssuer");

        public static string? TokenAudience => Environment.GetEnvironmentVariable("TokenAudience");
    }
}