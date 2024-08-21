// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Newtonsoft.Json;

namespace Aire.Id.Models;

public class PasswordChangeRequest
{
    [JsonProperty("current_password", Required = Required.Always)]
    public string? CurrentPassword { get; set; }

    [JsonProperty("new_password", Required = Required.Always)]
    public string? NewPassword { get; set; }
}
