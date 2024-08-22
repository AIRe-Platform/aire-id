using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Aire.Id.Oauth2.Models
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum OauthCodeChallengeMethod
	{
		[EnumMember(Value = "plain")]
		Plain,

		[EnumMember(Value = "S256")]
		SHA256
	}
}
