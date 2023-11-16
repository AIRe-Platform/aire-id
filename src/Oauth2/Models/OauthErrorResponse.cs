using Newtonsoft.Json;

namespace Aire.Id.Oauth2.Models
{
	public class OauthErrorResponse
	{
		[JsonProperty("error", Required = Required.Always)]
		public OauthError Error { get; set; } = OauthError.ServerError;

		[JsonProperty("error_description", NullValueHandling = NullValueHandling.Ignore)]
		public string ErrorDescription { get; set; } = null;

		[JsonProperty("error_uri", NullValueHandling = NullValueHandling.Ignore)]
		public string ErrorUri { get; set; } = null;

		[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
		public string State { get; set; } = null;
    }
}