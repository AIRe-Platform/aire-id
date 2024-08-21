// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Id.Oauth2.Models;

namespace Aire.Id.Oauth2.Providers
{
    public interface IOauthTokenProvider
    {
        string IssueNewToken(OauthTokenDescription description);
        OauthTokenResponse? GetTokenInfo(string token);
    }
}
