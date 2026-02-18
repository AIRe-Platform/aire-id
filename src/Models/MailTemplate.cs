// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


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

    public static class Invitation
    {
        public const string Id = "invitation";

        public static class Params
        {
            public const string Url = "url";
        }
    }
}
