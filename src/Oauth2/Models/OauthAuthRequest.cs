using Newtonsoft.Json;

namespace Aire.Id.Oauth2.Models
{
	public class OauthAuthRequest
	{
		[JsonProperty("response_type", Required = Required.Always)]
		public OauthResponseType ResponseType { get; set; }

		[JsonProperty("client_id", Required = Required.Always)]
		public string ClientId { get; set; }

		[JsonProperty("redirect_uri", NullValueHandling = NullValueHandling.Ignore)]
		public string RedirectUri { get; set; }

		[JsonProperty("scope", NullValueHandling = NullValueHandling.Ignore)]
		public string Scope { get; set; }

		[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)] // Recommended
		public string State { get; set; } = null;

		[JsonProperty("timestamp")]
		public long Timestamp { get; set; }
    }
}