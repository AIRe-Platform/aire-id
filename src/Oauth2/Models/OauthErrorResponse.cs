// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Sdk.Helpers;
using Newtonsoft.Json;

namespace Aire.Id.Oauth2.Models;

public class OauthErrorResponse
{
	[JsonProperty("error", Required = Required.Always)]
	public OauthError Error { get; set; } = OauthError.ServerError;

	[JsonProperty("error_description", NullValueHandling = NullValueHandling.Ignore)]
	public string? ErrorDescription { get; set; } = null;

	[JsonProperty("error_uri", NullValueHandling = NullValueHandling.Ignore)]
	public string? ErrorUri { get; set; } = null;

	[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
	public string? State { get; set; } = null;

	public Dictionary<string, string?> ToDictionary()
	{
		var dict = new Dictionary<string, string?>
			{
				{ "error", Error.ObjectToJson().Trim('"') }
			};

		if (ErrorDescription != null)
			dict.Add("error_description", ErrorDescription);

		if (ErrorUri != null)
			dict.Add("error_uri", ErrorUri);

		if (State != null)
			dict.Add("state", State);

		return dict;
	}
}
