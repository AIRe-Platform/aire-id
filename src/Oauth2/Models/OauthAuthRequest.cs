// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Sdk.AspNetCore;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Aire.Id.Oauth2.Models
{
	public class OauthAuthRequest
	{
		[JsonProperty("response_type", Required = Required.Always)]
		public OauthResponseType ResponseType { get; set; }

		[JsonProperty("client_id", Required = Required.Always)]
		public string? ClientId { get; set; }

		[JsonProperty("redirect_uri", NullValueHandling = NullValueHandling.Ignore)]
		public string? RedirectUri { get; set; }

		[JsonProperty("scope", NullValueHandling = NullValueHandling.Ignore)]
		public string? Scope { get; set; }

		[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)] // Recommended
		public string? State { get; set; } = null;

		public static OauthResponseType GetOauthResponseType(HttpRequest req)
		{
			string? grant = req.ReadParam("response_type");

			return grant switch
			{
				"code" => OauthResponseType.Code,
				"token" => OauthResponseType.Token,
				_ => throw new OauthException(OauthError.UnsupportedResponseType)
			};
		}

		public static OauthAuthRequest? FromRequest(HttpRequest req)
		{
			var auth_request = new OauthAuthRequest
			{
				ResponseType = GetOauthResponseType(req),
				ClientId = req.ReadParam("client_id"),
				RedirectUri = req.ReadParam("redirect_uri"),
				Scope = req.ReadParam("scope"),
				State = req.ReadParam("state")
			};

			if(string.IsNullOrWhiteSpace(auth_request.ClientId))
				throw new OauthException(OauthError.InvalidRequest);

			return auth_request;
		}
	}
}