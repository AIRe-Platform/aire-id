using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Aire.Id.Oauth2.Models
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum OauthTokenType
	{
		[EnumMember(Value = "bearer")]
		Bearer,

		[EnumMember(Value = "mac")]
		MAC
	}
}
