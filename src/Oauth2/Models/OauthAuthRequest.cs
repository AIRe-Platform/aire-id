// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Sdk.AspNetCore;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Aire.Id.Oauth2.Models;

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

	[JsonProperty("platform", NullValueHandling = NullValueHandling.Ignore)]
	public string? Platform { get; set; }

	[JsonProperty("code_challenge", NullValueHandling = NullValueHandling.Ignore)]
	public string? CodeChallenge { get; set; } = null;

	[JsonProperty("code_challenge_method", NullValueHandling = NullValueHandling.Ignore)]
	public OauthCodeChallengeMethod? CodeChallengeMethod { get; set; } = null;

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

	public static OauthCodeChallengeMethod? GetCodeChallengeMethod(HttpRequest req)
	{
		string? grant = req.ReadParam("code_challenge_method");

		return grant switch
		{
			"plain" => OauthCodeChallengeMethod.Plain,
			"S256" => OauthCodeChallengeMethod.SHA256,
			_ => null
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
			State = req.ReadParam("state"),
			CodeChallenge = req.ReadParam("code_challenge"),
			CodeChallengeMethod = GetCodeChallengeMethod(req),
			Platform = req.ReadParam("platform")
		};

		if (string.IsNullOrWhiteSpace(auth_request.ClientId))
			throw new OauthException(OauthError.InvalidRequest, auth_request);

		if (string.IsNullOrWhiteSpace(auth_request.CodeChallenge) || !auth_request.CodeChallengeMethod.HasValue)
			throw new OauthException(OauthError.InvalidRequest, auth_request);

		return auth_request;
	}
}
