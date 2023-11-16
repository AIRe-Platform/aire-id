using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Aire.Id.Oauth2.Models
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum OauthGrantType
	{
		[EnumMember(Value = "password")]
		Password,

		[EnumMember(Value = "client_credentials")]
		ClientCredentials,

		[EnumMember(Value = "authorization_code")]
		AuthorizationCode,

		[EnumMember(Value = "refresh_token")]
		RefreshToken
	}
}
