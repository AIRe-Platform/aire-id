using Newtonsoft.Json;

namespace Aire.Id.Oauth2.Models
{
	public class OauthAuthCodeResponse
	{
		[JsonProperty("code", Required = Required.Always)]
		public string? Code { get; set; }

		[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)] // Recommended
		public string? State { get; set; } = null;
    }
}