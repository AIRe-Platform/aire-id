using System;
using Aire.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Aire.Id.Oauth2.Models
{
	public class OauthTokenRequest
	{
		public OauthGrantType GrantType { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Scope { get; set; }
		public string Code { get; set; }
		public string RedirectUri { get; set; }
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }
		public string RefreshToken { get; set; }

		public OauthTokenRequest() {}

		public OauthTokenRequest(HttpRequest req)
		{
			if(Enum.TryParse<OauthGrantType>(req.ReadParam("grant_type"), out var grant))
				GrantType = grant;

			Username = req.ReadParam("username");
			Password = req.ReadParam("password");
			Scope = req.ReadParam("scope");
			Code = req.ReadParam("code");
			RedirectUri = req.ReadParam("redirect_uri");
			ClientId = req.ReadParam("client_id");
			ClientSecret = req.ReadParam("client_secret");
			RefreshToken = req.ReadParam("refresh_token");
		}
    }
}
