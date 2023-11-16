using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Aire.Id.Oauth2.Models
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum OauthResponseType
	{
		[EnumMember(Value = "code")]
		Code,

		[EnumMember(Value = "token")]
		Token
	}
}
