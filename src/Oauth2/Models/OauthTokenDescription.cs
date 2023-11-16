using System;

namespace Aire.Id.Oauth2.Models
{
    public class OauthTokenDescription
    {
        public OauthSubject Subject { get; set; }
        public TimeSpan Lifetime { get; set; }
    }
}
