// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


namespace Aire.Id.Oauth2.Models
{
    public class OauthSubject
    {
        public string? Subject { get; set; }
        public string? Role { get; set; }
        public List<string>? AllowedScopes { get; set; }
        public Dictionary<string, object>? Claims { get; set; }
        public bool Verified { get; set; }
    }
}