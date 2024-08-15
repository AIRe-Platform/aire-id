using Aire.Sdk.AspNetCore;
using Microsoft.AspNetCore.Http;

namespace Aire.Id.Oauth2.Models
{
	public class OauthTokenPasswordGrantRequest : OauthTokenRequest
	{
		// Required
		public string? Username { get; set; }

		// Required
		public string? Password { get; set; }

		// Optional
		public string? Scope { get; set; }

		public OauthTokenPasswordGrantRequest() : base(OauthGrantType.Password)
		{
		}

		public new static OauthTokenPasswordGrantRequest FromRequest(HttpRequest req)
		{
			var passwordGrantRequest = new OauthTokenPasswordGrantRequest
			{
				Username = req.ReadParam("username"),
				Password = req.ReadParam("password"),
				Scope = req.ReadParam("scope")
			};

			if (string.IsNullOrWhiteSpace(passwordGrantRequest.Username) ||
				string.IsNullOrWhiteSpace(passwordGrantRequest.Password))
			{
				throw new OauthException(OauthError.InvalidRequest);
			}

			return passwordGrantRequest;
		}
	}

	public class OauthTokenRefreshRequest : OauthTokenRequest
	{
		// Required
		public string? RefreshToken { get; set; }

		// Optional
		public string? Scope { get; set; }

		public OauthTokenRefreshRequest() : base(OauthGrantType.RefreshToken)
		{
		}

		public new static OauthTokenRefreshRequest FromRequest(HttpRequest req)
		{
			var refreshRequest = new OauthTokenRefreshRequest
			{
				RefreshToken = req.ReadParam("refresh_token"),
				Scope = req.ReadParam("scope")
			};

			if (string.IsNullOrWhiteSpace(refreshRequest.RefreshToken))
			{
				throw new OauthException(OauthError.InvalidRequest);
			}

			return refreshRequest;
		}
	}

	public class OauthAuthCodeGrantRequest : OauthTokenRequest
	{
		// Required
		public string? Code { get; set; }

		// Required
		public string? RedirectUri { get; set; }

		// Required
		public string? ClientId { get; set; }

		// Optional
		public string? ClientSecret { get; set; }

		// Optional
		public string? CodeVerifier { get; set; }

        public OauthAuthCodeGrantRequest() : base(OauthGrantType.AuthorizationCode)
		{
		}

		public new static OauthAuthCodeGrantRequest FromRequest(HttpRequest req)
		{
			var codeGrant = new OauthAuthCodeGrantRequest
			{
				Code = req.ReadParam("code"),
				RedirectUri = req.ReadParam("redirect_uri"),
				ClientId = req.ReadParam("client_id"),
				ClientSecret = req.ReadParam("client_secret"),
				CodeVerifier = req.ReadParam("code_verifier"),
				State = req.ReadParam("state")
			};

			if (string.IsNullOrWhiteSpace(codeGrant.Code) ||
				string.IsNullOrWhiteSpace(codeGrant.RedirectUri) ||
				string.IsNullOrWhiteSpace(codeGrant.ClientId))
			{
				throw new OauthException(OauthError.InvalidRequest);
			}

			return codeGrant;
		}
	}

	public abstract class OauthTokenRequest
	{
		// Required
		public OauthGrantType GrantType { get; private set; }

		// If set, required for the error response.
		public string? State { get; set; }

		public OauthTokenRequest(OauthGrantType grantType)
		{
			GrantType = grantType;
		}

		public static OauthGrantType? GetOauthGrantType(HttpRequest req)
		{
			string? grant = req?.ReadParam("grant_type");
			return grant switch
			{
				"password" => OauthGrantType.Password,
				"client_credentials" => OauthGrantType.ClientCredentials,
				"authorization_code" => OauthGrantType.AuthorizationCode,
				"refresh_token" => OauthGrantType.RefreshToken,
				_ => null
			};
		}

		public static OauthTokenRequest FromRequest(HttpRequest req)
		{
			var grant = GetOauthGrantType(req);
			return grant switch
			{
				OauthGrantType.Password => OauthTokenPasswordGrantRequest.FromRequest(req),
				OauthGrantType.RefreshToken => OauthTokenRefreshRequest.FromRequest(req),
				OauthGrantType.AuthorizationCode => OauthAuthCodeGrantRequest.FromRequest(req),
				// Client credentials unsupported
				_ => throw new OauthException(OauthError.UnsupportedGrantType),
			};
		}
	}
}
