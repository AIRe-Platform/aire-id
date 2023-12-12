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

	public abstract class OauthTokenRequest
	{
		// Required
		public OauthGrantType GrantType { get; private set; }

		// If set, required for the error response.
		public virtual string? State { get => null; }

		public OauthTokenRequest(OauthGrantType grantType)
		{
			GrantType = grantType;
		}

		public static OauthGrantType? GetOauthGrantType(HttpRequest req)
		{
			string? grant = req?.ReadParam("grant_type");
			
			return grant switch {
				"password" 				=> OauthGrantType.Password,
				"client_credentials" 	=> OauthGrantType.ClientCredentials,
				"authorization_code" 	=> OauthGrantType.AuthorizationCode,
				"refresh_token" 		=> OauthGrantType.RefreshToken,
				_ => null
			};
		}

		public static OauthTokenRequest FromRequest(HttpRequest req)
		{
			var grant = GetOauthGrantType(req);
            // TODO: Add other grant types
            return grant switch
            {
                OauthGrantType.Password => OauthTokenPasswordGrantRequest.FromRequest(req),
                _ => throw new OauthException(OauthError.UnsupportedGrantType),
            };
        }
    }
}
