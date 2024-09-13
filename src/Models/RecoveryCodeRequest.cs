// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Newtonsoft.Json;

namespace Aire.Id.Models;

public class RecoveryCodeRequest
{
    [JsonProperty("email", Required = Required.Always)]
    public string? Email { get; set; }

    [JsonProperty("language")]
    public string? Language { get; set; }

    public bool Validate()
    {
        return Helpers.Validation.IsValidEmail(Email);
    }
}
