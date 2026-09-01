// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Id.Helpers;
using Newtonsoft.Json;

namespace Aire.Id.Models;

public class RecoveryPasswordChangeRequest
{
    [JsonProperty("email", Required = Required.Always)]
    public string? Email { get; set; }

    [JsonProperty("code", Required = Required.Always)]
    public string? Code { get; set; }

    [JsonProperty("password", Required = Required.Always)]
    public string? Password { get; set; }

    [JsonProperty("language")]
    public string? Language { get; set; }

    public bool Validate(bool skipDomainWhitelist)
    {
        return Validation.IsValidEmail(Email, skipDomainWhitelist) && Validation.IsValidPassword(Password);
    }
}
