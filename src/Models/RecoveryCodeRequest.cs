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
