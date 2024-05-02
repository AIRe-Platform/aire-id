namespace Aire.Id.Models;

public class MailTemplate
{
    public string? TemplateName { get; set; }
    public string? Locale { get; set; }
    public string? Recipient { get; set; }
    public Dictionary<string, string>? Values { get; set; }

    public static class Verification
    {
        public const string Id = "verification";
        public static class Params
        {
            public const string Code = "code";
        }
    }

    public static class RecoveryCode
    {
        public const string Id = "recoveryCode";

        public static class Params
        {
            public const string Code = "code";
        }
    }

    public static class PasswordChanged
    {
        public const string Id = "passwordChanged";
    }
}
