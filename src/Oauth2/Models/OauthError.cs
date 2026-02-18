// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Aire.Id.Oauth2.Models;

[JsonConverter(typeof(StringEnumConverter))]
public enum OauthError
{
	// Missing or duplicate parameter(s), malformed request, etc.
	[EnumMember(Value = "invalid_request")]
	InvalidRequest,

	// The client is not authorized (using the method)
	[EnumMember(Value = "unauthorized_client")]
	UnauthorizedClient,

	// Request denied
	[EnumMember(Value = "access_denied")]
	AccessDenied,

	// Unsupported authorization method
	[EnumMember(Value = "unsupported_response_type")]
	UnsupportedResponseType,

	// Invalid, unknown or malformed scope
	[EnumMember(Value = "invalid_scope")]
	InvalidScope,

	// Unexpected error
	[EnumMember(Value = "server_error")]
	ServerError,

	// Overload or maintenance on server
	[EnumMember(Value = "temporarily_unavailable")]
	TemporarilyUnavailable,

	// Authorization grant is invalid, expired, revoked or does not match redirection uri
	[EnumMember(Value = "invalid_grant")]
	InvalidGrant,

	// The server does not support the grant type
	[EnumMember(Value = "unsupported_grant_type")]
	UnsupportedGrantType,
}
