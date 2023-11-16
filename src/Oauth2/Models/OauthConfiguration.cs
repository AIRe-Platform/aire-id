using System;
using System.Collections.Generic;

namespace Aire.Id.Oauth2
{
    public class OauthConfiguration
    {
        public List<string> Scopes { get; set; }
        public List<string> Roles { get; set; }
        public List<string> DefaultScopes { get; set; }
        public Dictionary<string, List<string>> DefaultRoleScopes { get; set; }
        public TimeSpan TokenLifetime { get; set; }
    }
}