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

    public bool Validate()
    {
        return Validation.IsValidEmail(Email) && Validation.IsValidPassword(Password);
    }
}
