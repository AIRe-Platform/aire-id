// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;

namespace Aire.Id.Models;

public class DemoGroupCreateRequest
{
    [JsonProperty("name", Required = Required.Always)]
    [OpenApiProperty(Description = "Demo group name")]
    public string? Name { get; set; }

    [JsonProperty("count", Required = Required.Always)]
    [OpenApiProperty(Description = "Number of users to generate in the group")]
    public int? Count { get; set; }

    [JsonProperty("username_prefix", Required = Required.Always)]
    [OpenApiProperty(Description = "Prefix to use when generating usernames")]
    public string? UsernamePrefix { get; set; }
}
