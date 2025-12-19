// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Newtonsoft.Json;

namespace Aire.Id.Oauth2.Models;

public class OauthAuthCodeResponse
{
	[JsonProperty("code", Required = Required.Always)]
	public string? Code { get; set; }

	[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)] // Recommended
	public string? State { get; set; } = null;
}
