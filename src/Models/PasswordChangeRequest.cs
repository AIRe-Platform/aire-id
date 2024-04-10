using Newtonsoft.Json;

namespace Aire.Id.Models;

public class PasswordChangeRequest
{
    [JsonProperty("current_password", Required = Required.Always)]
    public string? CurrentPassword { get; set; }

    [JsonProperty("new_password", Required = Required.Always)]
    public string? NewPassword { get; set; }
}
