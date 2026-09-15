// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Sdk.Auth;

namespace Aire.Id.Helpers;

public static class ScopeHelper
{
    public static AireScopes GetDefaultRoleScopes(string role)
    {
        if (AireScopes.DefaultRoleScopes.TryGetValue(role, out var scopes))
        {
            return scopes.GetMutableCopy();
        }
        return [];
    }
}
