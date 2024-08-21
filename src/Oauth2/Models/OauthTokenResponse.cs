// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Newtonsoft.Json;

namespace Aire.Id.Oauth2.Models
{
	public class OauthTokenResponse
	{
		[JsonProperty("access_token", Required = Required.Always)]
		public string? AccessToken { get; set; }

		// Return only when grant type type "password" was used.
		[JsonProperty("refresh_token", NullValueHandling = NullValueHandling.Ignore)]
		public string? RefreshToken { get; set; } = null;

		[JsonProperty("expires_in", Required = Required.Always)]
		public int ExpiresIn { get; set; }

		[JsonProperty("token_type", Required = Required.Always)]
		public OauthTokenType TokenType { get; set; } = OauthTokenType.Bearer;

		[JsonProperty("scope", NullValueHandling = NullValueHandling.Ignore)]
		public string? Scope { get; set; } = null;

		[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)] // Recommended
		public string? State { get; set; } = null;
    }
}