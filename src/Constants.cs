namespace Aire.Id;

public static class AireConstants
{
    public static class Queues
    {
        public const string Mail = "mail-queue";
    }

    public const int MaxVerificationRetryCount = 10;
    public const int MinPasswordLength = 8;

    public const string AppAuthPath = "/app/auth";
    public static readonly TimeSpan AppAuthSessionTTL = TimeSpan.FromDays(14);
}
