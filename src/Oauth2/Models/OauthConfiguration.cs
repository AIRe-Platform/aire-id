using System;
using System.Collections.Generic;
using Aire.Sdk.Auth.Scopes;

namespace Aire.Id.Oauth2
{
    public class OauthConfiguration
    {
        public AireScopes DefaultScopes { get; set; }
        public Dictionary<string, AireScopes> DefaultRoleScopes { get; set; }
        public TimeSpan TokenLifetime { get; set; }
    }
}