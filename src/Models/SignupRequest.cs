// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Newtonsoft.Json;

namespace Aire.Id.Models;

public class SignupCredentials
{
    [JsonProperty("email", Required = Required.Always)]
    public string? Email { get; set; }

    [JsonProperty("password", Required = Required.Always)]
    public string? Password { get; set; }
}

public class SignupRequest
{
    [JsonProperty("credentials", Required = Required.Always)]
    public SignupCredentials? Credentials { get; set; }

    [JsonProperty("platform", Required = Required.Always)]
    public string? Platform { get; set; }
}
