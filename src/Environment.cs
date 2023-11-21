namespace Aire
{
    public static class AireEnvironment
    {
        public static string StorageConnectionString {
            get => System.Environment.GetEnvironmentVariable("StorageConnectionString");
        }

        public static string TokenSigningKey {
            get => System.Environment.GetEnvironmentVariable("TokenSigningKey");
        }

        public static string TokenEncryptionKey {
            get => System.Environment.GetEnvironmentVariable("TokenEncryptionKey");
        }
        
        public static string TokenIssuer {
            get => System.Environment.GetEnvironmentVariable("TokenIssuer");
        }

        public static string TokenAudience {
            get => System.Environment.GetEnvironmentVariable("TokenAudience");
        }
    }
}