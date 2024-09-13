// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


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
