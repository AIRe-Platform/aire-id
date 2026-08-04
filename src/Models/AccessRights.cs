// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Aire.Id.Helpers;
using Aire.Sdk.Auth;
using Newtonsoft.Json;

namespace Aire.Id.Models;

public class AccessRights
{
    [JsonProperty("role")]
    public required string Role { get; set; }

    [JsonProperty("override_scopes")]
    public AireScopes? OverrideScopes { get; set; }

    [JsonProperty("additional_scopes")]
    public AireScopes? AdditionalScopes { get; set; }

    public AireScopes GetScopes(bool baseScopesOnly = false)
    {
        var scopes = OverrideScopes ?? ScopeHelper.GetDefaultRoleScopes(Role);

        if (AdditionalScopes != null && !baseScopesOnly)
            scopes.AddRange(AdditionalScopes);

        return scopes;
    }
}
