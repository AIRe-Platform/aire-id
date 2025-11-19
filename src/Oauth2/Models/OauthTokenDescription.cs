// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Sdk.Auth;

namespace Aire.Id.Oauth2.Models;

public class OauthTokenDescription(OauthSubject subject, AireScopes scopes, string? platform, TimeSpan lifetime)
{
    public OauthSubject Subject { get; set; } = subject;
    public AireScopes Scopes { get; set; } = scopes;
    public string? Platform { get; set; } = platform;
    public TimeSpan Lifetime { get; set; } = lifetime;
}

