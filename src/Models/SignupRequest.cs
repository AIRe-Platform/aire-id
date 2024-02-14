using Newtonsoft.Json;

namespace Aire.Id.Models
{
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
    }
}
